using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Collections;
using ICSharpCode.TextEditor.Document;
using System.IO;
using System.CodeDom.Compiler;

namespace Lexical_Analyzer_IDE
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        struct Identifiers
        {
            public String identifierName;
            public String identifierToken;

            public Identifiers(String idName, String idToken)
            {
                this.identifierName = idName;
                this.identifierToken = idToken;
            }
        }

        struct token
        {
            public String lexeme;
            public String tokenName;
            public token(String lexeme, String token)
            {
                this.lexeme = lexeme;
                this.tokenName = token;
            }
        }

        string[] liness;
        char[] tokenBreaker = { ' ', ';', '=', ',', '+', '-', '/', '*', '%', '.', '(', ')', '[', ']', '{', '}','<','>','&','|','!'};
        string[] doublelex = { "++","--","==",">=","<=","!=","&&","||","+=","-=","*=","/=","%="};
        string[] keyWord = { "var", "int", "String", "string", "using", "char", "void", "bool", "default", "in", "return", "true", "false", "try", "throw", "catch", "finally", "LinkedList", "new", "private", "public", "input", "inputln", "output", "outputln", "loop", "for", "foreach", "while", "do", "method", "if", "else", "then", "switch", "case", "break", "continue" };
        string[] operators = { "+", "-", "*", "/","%", "++", "--", "+=", "-=", "*=", "/=","%="};
        string[] relop = { "==", ">=", "<=", "!=", "=", "<", ">" ,"&&" };
        string[] punctuations = { ":", ",", ".", "'", '"' + "", ")", "(", "{", "}","[","]"};
        string terminator = ";";
        LinkedList<String> lexemes = new LinkedList<string>();
        LinkedList<token> tokens = new LinkedList<token>();
        LinkedList<Identifiers> identifier = new LinkedList<Identifiers>();
        Timer timer = new Timer();

        private void execute()
        {
            bool affirmation = false;
            pictureBox1.Visible = true;

            tokens.Clear();
            lexemes.Clear();
            identifier.Clear();

            affirmation = generateLines();
            if (affirmation)
            {
                affirmation = lexer();
            }
            if (affirmation)
            {
                affirmation = tokenizer();
            }
            if (affirmation)
            {
                tokenStream();
            }

            timer.Start();
        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode==Keys.F5 && tabControl1.SelectedIndex==0)
            {
                execute();
            }
        }

        private bool generateLines()
        {
            liness = new String[editor.Document.TotalNumberOfLines];
            for (int i = 0; i < liness.Length; i++)
            {
                String str = editor.Document.GetText(editor.Document.GetLineSegment(i));
                str = removeComment(str);
                str = str.Trim();
                str += '~';
                liness[i] = str;
            }
            return true;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            timer.Interval = 2000;
            timer.Tick += new EventHandler(timer_Tick);
        }

        private bool lexer()
        {
            int flag = 0;
            int flag1 = 0;
            lexemes.Clear();
            string lexeme = "";
            string doubleCheck = "";
            foreach (String line in liness)
            {
                foreach (char ch in line)
                {
                    if (ch == '~' && (flag==0 && flag1==0))
                    {
                        if (lexeme != "")
                        {
                            AddLex(lexeme);
                            lexeme = "";
                        }
                    }
                    else
                    {
                        if(ch=='"')
                        {
                            if (flag==0 && flag1==0)
	                        {
		                         flag=1;
	                        }
                            else if (flag==1)
                            {
                                flag=0;
                                AddLex(lexeme+ch);
                                lexeme = "";
                                continue;
                            }
                        }
                        if (ch==39)
                        {
                            if (flag1 == 0 && flag==0)
                            {
                                flag1 = 1;
                            }
                            else if (flag1==1)
                            {
                                flag1 = 0;
                                AddLex(lexeme + ch);
                                lexeme = "";
                                continue;
                            }
                        }
                        if(flag==1 || flag1==1)
                        {
                            lexeme+=ch;

                        }
                        else
                        {

                            if (isBreaker(ch))
                            {
                                if (lexeme != "")
                                {
                                    AddLex(lexeme);
                                    lexeme = "";
                                }
                                if (lexemes.Last!=null)
                                {
                                    doubleCheck = lexemes.Last.Value + ch;

                                }
                                if (containDoubleLex(doubleCheck))
                                {
                                    lexemes.RemoveLast();
                                    AddLex(doubleCheck);
                                }else if (ch != ' ')
                                {
                                    lexeme += ch;
                                    AddLex(lexeme);
                                    lexeme = "";
                                }
                            }
                            else
                            {
                                lexeme += ch;
                            }
                        }
                    }
                }
            }
            return true;
        }

        private void AddLex(String lex)
        {
            lex=lex.Trim();
            if (lex!=" ")
            {
                lexemes.AddLast(lex);
            }
        }

        private bool isBreaker(char ch) 
        {
            foreach (char cha in tokenBreaker)
            {
                if (cha==ch)
                {
                    return true;
                }
            }

            return false;
        }

        private string removeComment(string line)
        {
            String newStr = "";
            Char prevChar = '@';
            char[] arr = null;
            foreach (char ch in line)
            {
                if (ch == '/' && prevChar=='/')
                {
                    arr = newStr.ToCharArray();
                    newStr = newStr.Remove(arr.Length - 1);
                    break;
                }
                newStr += ch;
                prevChar=ch;

            }
            
            return newStr;
        }

        private void printLex()
        {
            dataGridView1.Rows.Clear();
            dataGridView1.Refresh(); 
            dataGridView2.Rows.Clear();
            dataGridView2.Refresh();

            for (int i = 0; i < tokens.Count; i++)
            {
                if (!(i==tokens.Count-1))
                {
                    dataGridView1.Rows.Add();
                }

                dataGridView1.Rows[i].Cells[1].Value = tokens.ElementAt<token>(i).lexeme;
                dataGridView1.Rows[i].Cells[2].Value = tokens.ElementAt<token>(i).tokenName;
            }

            for (int i = 0; i < identifier.Count; i++)
            {
                if (!(i == identifier.Count - 1))
                {
                    dataGridView2.Rows.Add();
                }
                dataGridView2.Rows[i].Cells[1].Value = identifier.ElementAt<Identifiers>(i).identifierName;
                dataGridView2.Rows[i].Cells[2].Value = identifier.ElementAt<Identifiers>(i).identifierToken;
            }

            for (int i = 0; i < dataGridView1.Rows.Count; i++)
            {
                dataGridView1.Rows[i].Cells[0].Value = i+1;
            }
            for (int i = 0; i < dataGridView2.Rows.Count; i++)
            {
                dataGridView2.Rows[i].Cells[0].Value = i+1;
            }

            toolStripStatusLabel1.Text = "Tokens Generated!..";
            toolStripStatusLabel2.Text = "Valid Token Count: " + tokens.Count;

        }

        private bool containDoubleLex(String str)
        {
            for (int i = 0; i < doublelex.Length; i++)
			{
                if (doublelex[i]==str)
                {
                    return true; 
                }
            }
            return false;
        }

        private bool tokenizer()
        {
            token newtoken=new token();
            tokens.Clear();
            int idCount = 0;
            foreach (String nLexemes in lexemes)
            {
                
                if(isKeyword(nLexemes))
                {
                    newtoken = new token(nLexemes,"Keyword");
                    tokens.AddLast(newtoken);

                }else if (isOperator(nLexemes))
                {
                    newtoken = new token(nLexemes, "Operator");
                    tokens.AddLast(newtoken);
                }
                else if (isRelop(nLexemes))
                {
                    newtoken = new token(nLexemes, "RelOp");
                    tokens.AddLast(newtoken);
                }
                else if (isPunctuation(nLexemes))
                {
                    newtoken = new token(nLexemes, "Punctuation");
                    tokens.AddLast(newtoken);
                }
                else if (isNumber(nLexemes))
                {
                    newtoken = new token(nLexemes, "NUM");
                    tokens.AddLast(newtoken);
                }
                else if (nLexemes == terminator)
                {
                    newtoken = new token(nLexemes, "Terminator");
                    tokens.AddLast(newtoken);
                }
                else if (isLiteral(nLexemes))
                {
                    newtoken = new token(nLexemes, "Literals");
                    tokens.AddLast(newtoken);
                }
                else if (isId(nLexemes))
                {

                    // using System.CodeDom.Compiler;
                    CodeDomProvider provider = CodeDomProvider.CreateProvider("C#");
                    if (provider.IsValidIdentifier(nLexemes))
                    {
                        int i = 0;
                        foreach (Identifiers id in identifier)
                        {
                            if (id.identifierName == nLexemes)
                            {
                                newtoken = new token(id.identifierName, id.identifierToken);
                                i = 1;
                            }
                        }
                        if (i == 0)
                        {
                            Identifiers newId = new Identifiers(nLexemes, "ID" + idCount);
                            newtoken = new token(nLexemes, "ID" + idCount);
                            identifier.AddLast(newId);
                            idCount++;
                        }
                        tokens.AddLast(newtoken);
                    }
                    else
                    {
                        MessageBox.Show("Invalid Identifier: "+nLexemes);
                    }
                }
                else
                {
                    toolStripStatusLabel1.Text = "Invalid Token";
                }
             }
            return true;
        }

        private bool tokenStream()
        {
            editor2.Text = "";
            foreach (token tok in tokens)
            {
                editor2.Text += tok.tokenName + ' ';
                if (tok.tokenName == "Terminator")
                {
                    editor2.Text += "\n";
                }
            }
            return true;
        }

        private bool isKeyword(string str)
        {
            for (int i = 0; i < keyWord.Length; i++)
            {
                if (keyWord[i] == str)
                {
                    return true;
                }
            }
            return false;
        }

        private bool isOperator(string str)
        {
            for (int i = 0; i < operators.Length; i++)
            {
                if (operators[i] == str)
                {
                    return true;
                }
            }
            return false;
        }

        private bool isRelop(string str)
        {
            for (int i = 0; i < relop.Length; i++)
            {
                if (relop[i] == str)
                {
                    return true;
                }
            }
            return false;
        }

        private bool isPunctuation(string str)
        {
            for (int i = 0; i < punctuations.Length; i++)
            {
                if (punctuations[i] == str)
                {
                    return true;
                }
            }
            return false;
        }

        private bool isNumber(string str)
        {
            double i = 0;
            if (double.TryParse(str, out i))
            {
                return true;
            }
            return false;
        }

        private bool isLiteral(string str)
        {
            if (str[0]=='"' && str[str.Length-1]=='"')
            {
                return true;
            }
            else if (str[0]==39 && str[str.Length-1]==39)
            {
                return true;
            }
            return false;
        }

        private bool isId(string str)
        {
            foreach (char ch in str)
            {
                if(isBreaker(ch))
                {
                    return false;
                    
                }
            }
            return true;
        }

        void timer_Tick(object sender, EventArgs e)
        {
            printLex();
            timer.Stop();
            pictureBox1.Visible = false;
        }

        private void editor_Load(object sender, EventArgs e)
        {
            string dir = Application.StartupPath;
            FileSyntaxModeProvider fsmp;
            if (Directory.Exists(dir))
            {
                fsmp = new FileSyntaxModeProvider(dir);
                HighlightingManager.Manager.AddSyntaxModeFileProvider(fsmp);
                editor.SetHighlighting("C#");

            }
        }

        private void editor2_Load(object sender, EventArgs e)
        {
            string dir = Application.StartupPath;
            FileSyntaxModeProvider fsmp;
            if (Directory.Exists(dir))
            {
                fsmp = new FileSyntaxModeProvider(dir);
                HighlightingManager.Manager.AddSyntaxModeFileProvider(fsmp);
                editor2.SetHighlighting("stream");

            }
        }
    }
}
