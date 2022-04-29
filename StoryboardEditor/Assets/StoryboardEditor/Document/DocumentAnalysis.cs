using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using StoryboardSystem;
using UnityEngine;

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

    public object Lock { get; } = new();

    private Table<CellAnalysis> newCells = new();
    private List<ProcedureInfo> newProcedures = new();

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
                
                if (i < Cells.Rows && j < Cells.Columns && value == Cells[i, j].Text) {
                    newCells[i, j] = Cells[i, j];
                    
                    continue;
                }

                if (value.StartsWith("//")) {
                    newCells[i, j] = new CellAnalysis(value, $"<color=#00FF00FF>{value}</color>", null, false);
                        
                    continue;
                }
                
                if (Parser.TryParseAndFormatToken(value, out var token, out string formatted))
                    newCells[i, j] = new CellAnalysis(value, formatted, token, false);
                else
                    newCells[i, j] = new CellAnalysis(value, $"<color=#FF0000FF>{value}</color>", null, true);
            }
        }
        
        newProcedures.Clear();

        var proceduresDict = new Dictionary<string, ProcedureInfo>();

        for (int i = 0; i < newCells.Rows; i++) {
            if (ct.IsCancellationRequested)
                return;

            var cell = newCells[i, 0];
            var token = cell.Token;
            
            if (token is not {Type: TokenType.Opcode} || ((OpcodeT) token).Opcode != Opcode.Proc || newCells.Columns < 2)
                continue;

            token = newCells[i, 1].Token;
            
            if (token is not {Type: TokenType.Chain})
                continue;

            var chain = (Chain) token;
            
            if (chain.Length != 1 || chain[0] is not Name name0) {
                newCells[i, 0] = new CellAnalysis(cell.Text, cell.FormattedText, cell.Token, true);
                
                continue;
            }

            string nameStr = name0.ToString();

            if (proceduresDict.ContainsKey(nameStr)) {
                newCells[i, 0] = new CellAnalysis(cell.Text, cell.FormattedText, cell.Token, true);
                
                continue;
            }
            
            var newArgNames = new List<string>();
            bool success = true;
            int rightMost = newCells.Columns;

            for (int j = newCells.Columns - 1; j >= 0; j--) {
                if (string.IsNullOrWhiteSpace(newCells[i, j].Text))
                    continue;

                rightMost = j;

                break;
            }

            for (int j = 2; j <= rightMost; j++) {
                cell = newCells[i, j];
                token = cell.Token;
                
                if (token is not {Type: TokenType.Chain}) {
                    newCells[i, j] = new CellAnalysis(cell.Text, cell.FormattedText, cell.Token, true);
                    success = false;
                    
                    continue;
                }

                chain = (Chain) token;
            
                if (chain.Length != 1 || chain[0] is not Name name1) {
                    newCells[i, j] = new CellAnalysis(cell.Text, cell.FormattedText, cell.Token, true);
                    success = false;
                    
                    continue;
                }

                newArgNames.Add(name1.ToString());
            }
            
            if (!success)
                newArgNames.Clear();

            var info = new ProcedureInfo(i, nameStr, newArgNames);
            
            newProcedures.Add(info);
            proceduresDict.Add(nameStr, info);
        }
        
        string[] argNames = new string[newCells.Columns];

        for (int i = 0; i < newCells.Rows; i++) {
            if (ct.IsCancellationRequested)
                return;
            
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
            
            names.CopyTo(argNames, 0);

            for (int j = 1, k = 0; j < newCells.Columns; j++, k++) {
                bool empty = string.IsNullOrWhiteSpace(newCells[i, j].Text);
                
                if (k < expectedLength && !empty || k >= expectedLength && (empty || opcode is Opcode.Call or Opcode.Loop or Opcode.Proc))
                    continue;

                var cell = newCells[i, j];
                
                if (empty)
                    newCells[i, j] = new CellAnalysis(cell.Text, $"<color=#FFFFFF40>{argNames[k]}</color>", cell.Token, true);
                else
                    newCells[i, j] = new CellAnalysis(cell.Text, cell.FormattedText, cell.Token, true);
            }

            if (opcode is not (Opcode.Call or Opcode.Loop))
                continue;
            
            int shift;

            if (opcode == Opcode.Call)
                shift = 3;
            else
                shift = 5;
                
            if (shift >= newCells.Columns)
                continue;

            token = newCells[i, 2].Token;
            
            if (token is not {Type: TokenType.Chain})
                continue;

            var chain = (Chain) token;
            
            if (chain.Length != 1 || chain[0] is not Name name || !proceduresDict.TryGetValue(name.ToString(), out var procedure))
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
        }

        callback?.Invoke();
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
