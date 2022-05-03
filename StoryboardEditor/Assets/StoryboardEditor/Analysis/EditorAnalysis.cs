using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using StoryboardSystem;
using UnityEngine;

public class EditorAnalysis {
    private static readonly string[] BIND_2 = { "controller", "property" };
    private static readonly string[] BUNDLE_2 = { "name", "fileName" };
    private static readonly string[] CALL_2 = { "time", "procedure" };
    private static readonly string[] CURVE_1 = { "name" };
    private static readonly string[] IN_2 = { "name", "key" };
    private static readonly string[] INST_2 = { "name", "template", "[parent", "layer]" };
    private static readonly string[] INST_4 = { "name", "template", "parent", "layer" };
    private static readonly string[] INSTA_3 = { "name", "count", "template", "[parent", "layer]" };
    private static readonly string[] INSTA_5 = { "name", "count", "template", "parent", "layer" };
    private static readonly string[] KEY_2 = { "time", "property", "[value]", "[interp]" };
    private static readonly string[] KEY_3 = { "time", "property", "value", "[interp]" };
    private static readonly string[] KEY_4 = { "time", "property", "value", "interp" };
    private static readonly string[] LOAD_3 = { "time", "name", "bundle", "fileName" };
    private static readonly string[] LOOP_4 = { "time", "procedure", "count", "every" };
    private static readonly string[] OUT_2 = { "key", "value" };
    private static readonly string[] POST_3 = { "name", "template", "camera" };
    private static readonly string[] PROC_1 = { "name" };
    private static readonly string[] SET_2 = { "name", "value" };
    private static readonly string[] SETA_2 = { "nameIdx", "value" };
    private static readonly string[] SETG_2 = { "name", "value" };

    public Table<CellAnalysis> Cells { get; private set; } = new();

    public List<ProcedureInfo> Procedures { get; private set; } = new();

    public Dictionary<string, VariableInfo> Globals { get; private set; } = new();

    public object Lock { get; } = new();

    private Table<CellAnalysis> newCells = new();
    private List<ProcedureInfo> newProcedures = new();
    private Dictionary<string, VariableInfo> newGlobals = new();

    private CancellationTokenSource ctSource;
    private Task activeTask;

    public void Analyze(Table<string> document, Action callback) {
        ctSource = new CancellationTokenSource();

        var ct = ctSource.Token;
        
        activeTask = Task.Run(() => AnalyzeAsync(document, callback, ct), ct);
    }

    public void Cancel() {
        ctSource?.Cancel();
        activeTask?.Wait();

        ctSource = null;
        activeTask = null;
    }

    private void AnalyzeAsync(Table<string> document, Action callback, CancellationToken ct) {
        if (!FillCells(document, ct))
            return;

        var proceduresDict = new Dictionary<string, ProcedureInfo>();

        if (!FillVariablesAndProcedures(proceduresDict, ct)
            || !FillUsagesAndValidateCells(proceduresDict, ct)
            || ct.IsCancellationRequested)
            return;
        
        lock (Lock) {
            (Cells, newCells) = (newCells, Cells);
            (Procedures, newProcedures) = (newProcedures, Procedures);
            (Globals, newGlobals) = (newGlobals, Globals);
        }

        callback?.Invoke();
    }

    private bool FillCells(Table<string> document, CancellationToken ct) {
        newCells.SetSize(document.Rows, document.Columns);

        for (int i = 0; i < document.Rows; i++) {
            if (ct.IsCancellationRequested)
                return false;

            for (int j = 0; j < document.Columns; j++) {
                string value = document[i, j];

                if (i < Cells.Rows && j < Cells.Columns) {
                    var oldCell = Cells[i, j];

                    if (oldCell != null && value == oldCell.Text) {
                        newCells[i, j] = oldCell;

                        continue;
                    }
                }

                if (string.IsNullOrWhiteSpace(value)) {
                    newCells[i, j] = new CellAnalysis(string.Empty, string.Empty, null, false);

                    continue;
                }

                value = value.Trim();

                if (value.StartsWith("//")) {
                    newCells[i, j] = new CellAnalysis(value, $"<color=#00FF00FF>{value}</color>", null, false);

                    continue;
                }

                bool isError = !Parser.TryParseToken(new StringRange(value), i, new DummyLogger(), true, out var token);

                if (token == null) {
                    newCells[i, j] = new CellAnalysis(value, $"<color=#FF0000FF>{value}</color>", null, true);

                    continue;
                }

                var cell = new CellAnalysis(value, token.FormattedText, token, isError);

                Traverse(token);
                newCells[i, j] = cell;

                void Traverse(Token token) {
                    cell.Tokens.Add(token);

                    switch (token.Type) {
                        case TokenType.Array:
                            foreach (var subToken in (ArrayT) token)
                                Traverse(subToken);

                            return;
                        case TokenType.Chain:
                            foreach (var subToken in (Chain) token)
                                Traverse(subToken);

                            return;
                        case TokenType.FuncCall:
                            foreach (var argument in ((FuncCall) token).Arguments)
                                Traverse(argument);

                            return;
                        case TokenType.Indexer:
                            Traverse(((Indexer) token).Token);

                            return;
                        case TokenType.Invalid:
                        case TokenType.Constant:
                        case TokenType.Name:
                        case TokenType.Opcode:
                        default:
                            return;
                    }
                }
            }
        }

        return true;
    }

    private bool FillVariablesAndProcedures(Dictionary<string, ProcedureInfo> proceduresDict, CancellationToken ct) {
        int currentProcedureIndex = -1;
        string currentProcedureName = string.Empty;
        var currentProcedureArgNames = new List<string>();
        var currentProcedureLocals = new Dictionary<string, VariableInfo>();
        
        newProcedures.Clear();
        newGlobals.Clear();

        for (int i = 0; i < newCells.Rows; i++) {
            if (ct.IsCancellationRequested)
                return false;

            var cell = newCells[i, 0];
            var token = cell.Token;

            if (token is not { Type: TokenType.Opcode } || newCells.Columns < 2)
                continue;

            switch (((OpcodeT) token).Opcode) {
                case Opcode.Proc: {
                    if (currentProcedureIndex >= 0)
                        PopProcedure();

                    currentProcedureIndex = i;
                    currentProcedureArgNames = new List<string>();
                    currentProcedureLocals = new Dictionary<string, VariableInfo>() { { "count", new VariableInfo("count", new Vector2Int(i, -1)) }, { "iter", new VariableInfo("iter", new Vector2Int(i, -1)) } };

                    cell = newCells[i, 1];

                    if (!TryGetName(cell.Token, out currentProcedureName) || proceduresDict.ContainsKey(currentProcedureName)) {
                        cell.IsError = true;
                        currentProcedureIndex = -1;

                        continue;
                    }

                    int rightMost = newCells.Columns;

                    for (int j = newCells.Columns - 1; j >= 0; j--) {
                        if (string.IsNullOrWhiteSpace(newCells[i, j].Text))
                            continue;

                        rightMost = j;

                        break;
                    }

                    for (int j = 2; j <= rightMost; j++) {
                        cell = newCells[i, j];

                        if (!TryGetName(cell.Token, out string argName)) {
                            cell.IsError = true;
                            
                            continue;
                        }

                        currentProcedureArgNames.Add(argName);
                        currentProcedureLocals.Add(argName, new VariableInfo(argName, new Vector2Int(i, j)));
                    }

                    continue;
                }
                case Opcode.Bundle:
                case Opcode.Curve:
                case Opcode.In:
                case Opcode.Inst:
                case Opcode.InstA:
                case Opcode.Load:
                case Opcode.Post:
                case Opcode.SetG: {
                    cell = newCells[i, 1];

                    if (!TryGetName(cell.Token, out string globalName)) {
                        cell.IsError = true;

                        continue;
                    }
                    
                    if (!newGlobals.ContainsKey(globalName))
                        newGlobals.Add(globalName, new VariableInfo(globalName, new Vector2Int(i, 1)));

                    continue;
                }
                case Opcode.Set: {
                    cell = newCells[i, 1];

                    if (!TryGetName(cell.Token, out string localName)) {
                        cell.IsError = true;

                        continue;
                    }

                    if (!currentProcedureLocals.ContainsKey(localName))
                        currentProcedureLocals.Add(localName, new VariableInfo(localName, new Vector2Int(i, 1)));

                    continue;
                }
                case Opcode.Bind:
                case Opcode.Call:
                case Opcode.Key:
                case Opcode.Loop:
                case Opcode.Out:
                case Opcode.SetA:
                default:
                    continue;
            }
        }

        if (currentProcedureIndex >= 0)
            PopProcedure();

        return true;

        void PopProcedure() {
            var info = new ProcedureInfo(currentProcedureIndex, currentProcedureName, currentProcedureArgNames, currentProcedureLocals);

            newProcedures.Add(info);

            if (newGlobals.TryGetValue(currentProcedureName, out var globalInfo))
                globalInfo.Declaration = new Vector2Int(currentProcedureIndex, 1);
            else
                newGlobals.Add(currentProcedureName, new VariableInfo(currentProcedureName, new Vector2Int(currentProcedureIndex, 1)));

            proceduresDict.Add(currentProcedureName, info);
        }
    }

    private bool FillUsagesAndValidateCells(Dictionary<string, ProcedureInfo> proceduresDict, CancellationToken ct) {
        string[] argNames = new string[newCells.Columns];
        var currentProcedure = new ProcedureInfo(-1, null, null, null);
        int currentProcedureIndex = -1;

        for (int i = 0; i < newCells.Rows; i++) {
            if (ct.IsCancellationRequested)
                return false;

            if (currentProcedureIndex + 1 < newProcedures.Count && i >= newProcedures[currentProcedureIndex + 1].Index) {
                var nextProcedure = newProcedures[currentProcedureIndex + 1];

                if (i >= nextProcedure.Index) {
                    currentProcedureIndex++;
                    currentProcedure = nextProcedure;
                }
            }

            int lastFilled = -1;

            for (int j = newCells.Columns - 1; j >= 0; j--) {
                if (string.IsNullOrWhiteSpace(newCells[i, j].Text) || newCells[i, j].Token == null)
                    continue;

                lastFilled = j;

                break;
            }

            if (lastFilled < 0)
                continue;

            var cell = newCells[i, 0];
            var token = cell.Token;

            if (token is not { Type: TokenType.Opcode }) {
                cell.IsError = true;

                continue;
            }

            var opcode = ((OpcodeT) token).Opcode;
            int expectedLength = GetExpectedArgumentsForInstruction(opcode, lastFilled, out string[] names);
            bool unlimited = opcode is Opcode.Call or Opcode.Loop or Opcode.Proc;

            names.CopyTo(argNames, 0);

            for (int j = 1, k = 0; j < newCells.Columns; j++, k++) {
                cell = newCells[i, j];

                bool empty = string.IsNullOrWhiteSpace(cell.Text);

                if ((k < expectedLength && empty) || (k >= expectedLength && !empty && !unlimited)) {
                    if (empty)
                        cell.FormattedText = $"<color=#FFFFFF40>{argNames[k]}</color>";

                    cell.IsError = true;

                    continue;
                }

                if (empty)
                    continue;

                for (int l = 0; l < cell.Tokens.Count; l++) {
                    var cellToken = cell.Tokens[l];

                    if (ValidateToken(cellToken, l))
                        continue;

                    cell.IsError = true;

                    break;
                }

                bool ValidateToken(Token token, int index) {
                    switch (token.Type) {
                        case TokenType.Chain:
                            var chain = (Chain) token;

                            if (chain.Length == 0 || chain[0].Type != TokenType.Name)
                                return false;

                            string name = ((Name) chain[0]).ToString();

                            if (!newGlobals.TryGetValue(name, out var info)
                                && (currentProcedure.Index < 0
                                    || !currentProcedure.Locals.TryGetValue(name, out info)
                                    || i < info.Declaration.x
                                    || i == info.Declaration.x && j != info.Declaration.y))
                                return false;

                            info.Usages.Add(new VariableUsage(i, j, index + 1));

                            return true;
                        case TokenType.Array:
                        case TokenType.Constant:
                        case TokenType.FuncCall:
                        case TokenType.Indexer:
                        case TokenType.Name:
                            return true;
                        case TokenType.Invalid:
                        case TokenType.Opcode:
                        default:
                            return false;
                    }
                }
            }

            if (opcode is not (Opcode.Call or Opcode.Loop))
                continue;

            int shift;

            if (opcode == Opcode.Call)
                shift = 3;
            else
                shift = 5;

            if (shift >= newCells.Columns || !TryGetName(newCells[i, 2].Token, out string name) || !proceduresDict.TryGetValue(name, out var procedure))
                continue;

            var procArgNames = procedure.ArgNames;

            for (int j = 0, k = shift; j < procArgNames.Count && k < newCells.Columns; j++, k++) {
                cell = newCells[i, k];

                if (string.IsNullOrWhiteSpace(cell.Text))
                    cell.FormattedText = $"<color=#FFFFFF40>{procArgNames[j]}</color>";
            }
        }

        return true;
    }

    private static bool TryGetName(Token token, out string name) {
        if (token is not { Type: TokenType.Chain }) {
            name = null;

            return false;
        }

        var chain = (Chain) token;

        if (chain.Length != 1) {
            name = null;

            return false;
        }

        var first = chain[0];
        
        if (first.Type != TokenType.Name) {
            name = null;

            return false;
        }

        name = ((Name) first).ToString();

        if (!string.IsNullOrWhiteSpace(name))
            return true;
        
        name = null;

        return false;
    }

    private static int GetExpectedArgumentsForInstruction(Opcode opcode, int lastFilled, out string[] names) {
        switch (opcode, lastFilled) {
            case (Opcode.Bind, _):
                names = BIND_2;
                return 2;
            case (Opcode.Bundle, _):
                names = BUNDLE_2;
                return 2;
            case (Opcode.Call, _):
                names = CALL_2;
                return 2;
            case (Opcode.Curve, _):
                names = CURVE_1;
                return 1;
            case (Opcode.In, _):
                names = IN_2;
                return 2;
            case (Opcode.Inst, <= 2):
                names = INST_2;
                return 2;
            case (Opcode.Inst, > 2):
                names = INST_4;
                return 4;
            case (Opcode.InstA, <= 3):
                names = INSTA_3;
                return 3;
            case (Opcode.InstA, > 3):
                names = INSTA_5;
                return 5;
            case (Opcode.Key, <= 2):
                names = KEY_2;
                return 2;
            case (Opcode.Key, 3):
                names = KEY_3;
                return 3;
            case (Opcode.Key, > 3):
                names = KEY_4;
                return 4;
            case (Opcode.Load, _):
                names = LOAD_3;
                return 3;
            case (Opcode.Loop, _):
                names = LOOP_4;
                return 4;
            case (Opcode.Out, _):
                names = OUT_2;
                return 2;
            case (Opcode.Post, _):
                names = POST_3;
                return 3;
            case (Opcode.Proc, _):
                names = PROC_1;
                return 1;
            case (Opcode.Set, _):
                names = SET_2;
                return 2;
            case (Opcode.SetA, _):
                names = SETA_2;
                return 2;
            case (Opcode.SetG, _):
                names = SETG_2;
                return 2;
            default:
                throw new ArgumentOutOfRangeException(nameof(opcode), opcode, null);
        }
    }
}
