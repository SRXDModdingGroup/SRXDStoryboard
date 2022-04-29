using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using StoryboardSystem;

public class DocumentAnalysis {
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

    public HashSet<string> Globals { get; private set; } = new();

    public object Lock { get; } = new();

    private Table<CellAnalysis> newCells = new();
    private List<ProcedureInfo> newProcedures = new();
    private HashSet<string> newGlobals = new();

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
        newCells.SetSize(document.Rows, document.Columns);

        for (int i = 0; i < document.Rows; i++) {
            if (ct.IsCancellationRequested)
                return;
            
            for (int j = 0; j < document.Columns; j++) {
                string value = document[i, j];

                if (string.IsNullOrWhiteSpace(value)) {
                    newCells[i, j] = new CellAnalysis(string.Empty, string.Empty, null, false);
                        
                    continue;
                }

                value = value.Trim();

                if (value.StartsWith("//")) {
                    newCells[i, j] = new CellAnalysis(value, $"<color=#00FF00FF>{value}</color>", null, false);
                        
                    continue;
                }

                Token token;
                string formatted;

                if (i < Cells.Rows && j < Cells.Columns && value == Cells[i, j].Text) {
                    var cell = Cells[i, j];
                    
                    token = cell.Token;
                    formatted = cell.FormattedText;
                }
                else if (!Parser.TryParseAndFormatToken(value, out token, out formatted))
                    token = null;

                if (token == null)
                    newCells[i, j] = new CellAnalysis(value, $"<color=#FF0000FF>{value}</color>", null, true);
                else
                    newCells[i, j] = new CellAnalysis(value, formatted, token, false);
            }
        }
        
        newProcedures.Clear();
        newGlobals.Clear();

        var proceduresDict = new Dictionary<string, ProcedureInfo>();
        int currentProcedureIndex = -1;
        string currentProcedureName = string.Empty;
        var currentProcedureArgNames = new List<string>();
        var currentProcedureLocals = new Dictionary<string, int>();

        for (int i = 0; i < newCells.Rows; i++) {
            if (ct.IsCancellationRequested)
                return;

            var cell = newCells[i, 0];
            var token = cell.Token;
            
            if (token is not {Type: TokenType.Opcode} || newCells.Columns < 2) {
                for (int j = 1; j < newCells.Columns; j++) {
                    if (newCells[i, j].Token == null)
                        continue;
                    
                    newCells[i, 0] = new CellAnalysis(cell.Text, cell.FormattedText, cell.Token, true);

                    break;
                }
                
                continue;
            }

            switch (((OpcodeT) token).Opcode) {
                case Opcode.Proc: {
                    if (currentProcedureIndex >= 0) {
                        var info = new ProcedureInfo(currentProcedureIndex, currentProcedureName, currentProcedureArgNames, currentProcedureLocals);

                        newProcedures.Add(info);
                        newGlobals.Add(info.Name);
                        proceduresDict.Add(currentProcedureName, info);
                    }

                    currentProcedureIndex = i;
                    currentProcedureArgNames = new List<string>();
                    currentProcedureLocals = new Dictionary<string, int>() {{ "count", i }, { "iter", i }};

                    cell = newCells[i, 1];

                    if (!TryGetName(cell.Token, out currentProcedureName) || proceduresDict.ContainsKey(currentProcedureName)) {
                        newCells[i, 1] = new CellAnalysis(cell.Text, cell.FormattedText, cell.Token, true);
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
                            newCells[i, j] = new CellAnalysis(cell.Text, cell.FormattedText, cell.Token, true);
                            currentProcedureIndex = -1;

                            continue;
                        }

                        currentProcedureArgNames.Add(argName);
                        currentProcedureLocals.Add(argName, i);
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
                        newCells[i, 1] = new CellAnalysis(cell.Text, cell.FormattedText, cell.Token, true);

                        continue;
                    }
                    
                    newGlobals.Add(globalName);

                    continue;
                }
                case Opcode.Set: {
                    cell = newCells[i, 1];

                    if (!TryGetName(cell.Token, out string localName)) {
                        newCells[i, 1] = new CellAnalysis(cell.Text, cell.FormattedText, cell.Token, true);

                        continue;
                    }
                    
                    if (!currentProcedureLocals.ContainsKey(localName))
                        currentProcedureLocals.Add(localName, i);

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
        
        if (currentProcedureIndex >= 0) {
            var info = new ProcedureInfo(currentProcedureIndex, currentProcedureName, currentProcedureArgNames, currentProcedureLocals);

            newProcedures.Add(info);
            newGlobals.Add(info.Name);
            proceduresDict.Add(currentProcedureName, info);
        }
        
        string[] argNames = new string[newCells.Columns];
        var currentProcedure = new ProcedureInfo(-1, null, null, null);
        
        currentProcedureIndex = -1;

        for (int i = 0; i < newCells.Rows; i++) {
            if (ct.IsCancellationRequested)
                return;

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

            var first = newCells[i, 0];
            var token = first.Token;

            if (token is not { Type: TokenType.Opcode }) {
                newCells[i, 0] = new CellAnalysis(first.Text, first.FormattedText, first.Token, true);
                
                continue;
            }

            var opcode = ((OpcodeT) token).Opcode;
            int expectedLength = GetExpectedArgumentsForInstruction(opcode, lastFilled, out string[] names);
            bool unlimited = opcode is Opcode.Call or Opcode.Loop or Opcode.Proc;

            names.CopyTo(argNames, 0);

            for (int j = 1, k = 0; j < newCells.Columns; j++, k++) {
                var cell = newCells[i, j];
                bool empty = string.IsNullOrWhiteSpace(cell.Text);

                if ((k < expectedLength && empty) || (k >= expectedLength && !empty && !unlimited)) {
                    if (empty)
                        newCells[i, j] = new CellAnalysis(cell.Text, $"<color=#FFFFFF40>{argNames[k]}</color>", cell.Token, true);
                    else
                        newCells[i, j] = new CellAnalysis(cell.Text, cell.FormattedText, cell.Token, true);
                    
                    continue;
                }

                if (!empty && !ValidateToken(cell.Token))
                    newCells[i, j] = new CellAnalysis(cell.Text, cell.FormattedText, cell.Token, true);

                bool ValidateToken(Token token) {
                    switch (token.Type) {
                        case TokenType.Array:
                            var array = (ArrayT) token;

                            for (int l = 0; l < array.Length; l++) {
                                if (!ValidateToken(array[l]))
                                    return false;
                            }

                            return true;
                        case TokenType.Chain:
                            var chain = (Chain) token;

                            if (chain.Length == 0 || chain[0].Type != TokenType.Name)
                                return false;
                            
                            string name = ((Name) chain[0]).ToString();
                            
                            if (!newGlobals.Contains(name)
                                && (currentProcedure.Index < 0 || !currentProcedure.Locals.TryGetValue(name, out int localIndex) || i < localIndex))
                                return false;

                            for (int l = 0; l < chain.Length; l++) {
                                if (!ValidateToken(chain[l]))
                                    return false;
                            }

                            return true;
                        case TokenType.Constant:
                            return true;
                        case TokenType.FuncCall:
                            foreach (var argToken in ((FuncCall) token).Arguments) {
                                if (!ValidateToken(argToken))
                                    return false;
                            }

                            return true;
                        case TokenType.Indexer:
                            return ValidateToken(((Indexer) token).Token);
                        case TokenType.Name:
                            return !string.IsNullOrWhiteSpace(((Name) token).ToString());
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
                var cell = newCells[i, k];

                if (string.IsNullOrWhiteSpace(cell.Text))
                    newCells[i, k] = new CellAnalysis(cell.Text, $"<color=#FFFFFF40>{procArgNames[j]}</color>", cell.Token, cell.IsError);
            }
        }
        
        if (ct.IsCancellationRequested)
            return;
        
        lock (Lock) {
            (Cells, newCells) = (newCells, Cells);
            (Procedures, newProcedures) = (newProcedures, Procedures);
            (Globals, newGlobals) = (newGlobals, Globals);
        }

        callback?.Invoke();
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
