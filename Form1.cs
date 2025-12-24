using System.Drawing;
using System.Windows.Forms;

namespace CalculatorApp;

public partial class Form1 : Form
{
    private TextBox displayBox;
    private double currentResult = 0;
    private double currentInput = 0;
    private string currentOperator = "";
    private bool isNewEntry = true;
    private bool isDegreeMode = true; // Default to Degrees

    private double memoryValue = 0;

    private int themeMode = 0; // 0: Light, 1: Dark, 2: Jazz
    private Button themeBtn;
    
    // Side Panel Controls
    private Panel sidePanel;
    private Button showHistoryBtn;
    private Button showConverterBtn;
    private Button showHelpBtn;
    
    // Help Controls
    private Panel helpPanel;
    private RichTextBox helpText;
    
    // History Controls
    private Panel historyPanel;
    private ListBox historyList;
    private Button clearHistoryBtn;
    private Button exportHistoryBtn;

    // Converter Controls
    private Panel converterPanel;
    private ComboBox conversionType;
    private TextBox inputUnitBox;
    private TextBox outputUnitBox;
    private Label eqLabel;
    private ComboBox fromUnit;
    private ComboBox toUnit;
    private Button convertBtn;


    private string historyFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "CalculatorHistory.txt");

    public Form1()
    {
        InitializeComponent();
        InitializeCalculatorUI();
        this.KeyPreview = true;
        this.KeyDown += Form1_KeyDown;
        this.FormClosing += Form1_FormClosing;
        LoadHistory();
    }

    private void Form1_FormClosing(object sender, FormClosingEventArgs e)
    {
        SaveHistory();
    }

    private void SaveHistory()
    {
        try
        {
            File.WriteAllLines(historyFilePath, historyList.Items.Cast<string>());
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Failed to save history: {ex.Message}");
        }
    }

    private void LoadHistory()
    {
        try
        {
            if (File.Exists(historyFilePath))
            {
                string[] lines = File.ReadAllLines(historyFilePath);
                historyList.Items.AddRange(lines);
            }
        }
        catch { /* Ignore load errors */ }
    }

    // Layout Containers
    private TableLayoutPanel mainLayout;
    private TableLayoutPanel buttonGrid;
    private Panel rightDockPanel;
    private TableLayoutPanel sideToggleGrid;
    private Panel sideContentPanel;

    private void InitializeCalculatorUI()
    {
        this.Text = "Scientific Calculator";
        this.Size = new Size(900, 600);
        this.MinimumSize = new Size(600, 400);
        this.FormBorderStyle = FormBorderStyle.Sizable; // Allow resizing
        this.MaximizeBox = true; // Enable Maximize
        this.WindowState = FormWindowState.Maximized; // Start Maximized
        this.StartPosition = FormStartPosition.CenterScreen;

        InitializeRightPanel();
        InitializeMainLayout();

        // Initialize content
        ConversionType_Changed(null, null);
        ToggleSidePanel("History"); // Default
    }

    private void InitializeRightPanel()
    {
        // --- Right Side Panel (Fixed Width) ---
        rightDockPanel = new Panel();
        rightDockPanel.Dock = DockStyle.Right;
        rightDockPanel.Width = 300;
        this.Controls.Add(rightDockPanel);

        // Side Panel Toggles (Top of Right Panel)
        sideToggleGrid = new TableLayoutPanel();
        sideToggleGrid.Dock = DockStyle.Top;
        sideToggleGrid.Height = 40;
        sideToggleGrid.ColumnCount = 3;
        sideToggleGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33F));
        sideToggleGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33F));
        sideToggleGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33F));
        rightDockPanel.Controls.Add(sideToggleGrid);

        showHistoryBtn = new Button() { Text = "History", Dock = DockStyle.Fill, Margin = new Padding(0) };
        showHistoryBtn.Click += (s, e) => ToggleSidePanel("History");
        sideToggleGrid.Controls.Add(showHistoryBtn, 0, 0);

        showConverterBtn = new Button() { Text = "Converter", Dock = DockStyle.Fill, Margin = new Padding(0) };
        showConverterBtn.Click += (s, e) => ToggleSidePanel("Converter");
        sideToggleGrid.Controls.Add(showConverterBtn, 1, 0);

        showHelpBtn = new Button() { Text = "Help", Dock = DockStyle.Fill, Margin = new Padding(0) };
        showHelpBtn.Click += (s, e) => ToggleSidePanel("Help");
        sideToggleGrid.Controls.Add(showHelpBtn, 2, 0);

        // Theme Button (Bottom of Right Panel)
        themeBtn = new Button();
        themeBtn.Text = "Change Theme";
        themeBtn.Dock = DockStyle.Bottom;
        themeBtn.Height = 40;
        themeBtn.Click += Theme_Click;
        rightDockPanel.Controls.Add(themeBtn);

        // Side Content PlaceHolder (Fill Right Panel)
        sideContentPanel = new Panel();
        sideContentPanel.Dock = DockStyle.Fill;
        rightDockPanel.Controls.Add(sideContentPanel);
        sideContentPanel.BringToFront(); // Ensure it's between Top and Bottom

        // --- History Content Init ---
        historyPanel = new Panel();
        historyPanel.Dock = DockStyle.Fill;

        historyList = new ListBox();
        historyList.Dock = DockStyle.Fill;
        historyList.Font = new Font("Segoe UI", 10, FontStyle.Regular);
        historyPanel.Controls.Add(historyList);
        
        Panel historyActionPanel = new Panel() { Dock = DockStyle.Bottom, Height = 40 };
        
        clearHistoryBtn = new Button() { Text = "Clear", Dock = DockStyle.Left, Width = 140 };
        clearHistoryBtn.Click += (s, e) => historyList.Items.Clear();
        
        exportHistoryBtn = new Button() { Text = "Export", Dock = DockStyle.Right, Width = 140 };
        exportHistoryBtn.Click += ExportHistory_Click;
        
        historyActionPanel.Controls.Add(clearHistoryBtn);
        historyActionPanel.Controls.Add(exportHistoryBtn);
        historyPanel.Controls.Add(historyActionPanel);

        // --- Converter Content Init ---
        converterPanel = new Panel();
        converterPanel.Dock = DockStyle.Fill;

        TableLayoutPanel convLayout = new TableLayoutPanel();
        convLayout.Dock = DockStyle.Fill;
        convLayout.Padding = new Padding(10);
        convLayout.RowCount = 8;
        
        convLayout.Controls.Add(new Label() { Text = "Type:", AutoSize = true }, 0, 0);
        
        conversionType = new ComboBox() { DropDownStyle = ComboBoxStyle.DropDownList, Dock = DockStyle.Top };
        conversionType.Items.AddRange(new object[] { "Length", "Weight", "Temperature", "Data" });
        conversionType.SelectedIndex = 0;
        conversionType.SelectedIndexChanged += ConversionType_Changed;
        convLayout.Controls.Add(conversionType, 0, 1);

        inputUnitBox = new TextBox() { Text = "1", Dock = DockStyle.Top };
        convLayout.Controls.Add(inputUnitBox, 0, 2);

        fromUnit = new ComboBox() { DropDownStyle = ComboBoxStyle.DropDownList, Dock = DockStyle.Top };
        convLayout.Controls.Add(fromUnit, 0, 3);

        eqLabel = new Label() { Text = "=", AutoSize = true, Font = new Font("Segoe UI", 12, FontStyle.Bold), TextAlign = ContentAlignment.MiddleCenter, Dock = DockStyle.Top };
        convLayout.Controls.Add(eqLabel, 0, 4);

        outputUnitBox = new TextBox() { ReadOnly = true, Dock = DockStyle.Top };
        convLayout.Controls.Add(outputUnitBox, 0, 5);

        toUnit = new ComboBox() { DropDownStyle = ComboBoxStyle.DropDownList, Dock = DockStyle.Top };
        convLayout.Controls.Add(toUnit, 0, 6);

        convertBtn = new Button() { Text = "Convert", BackColor = Color.LightGreen, Dock = DockStyle.Top, Height = 40 };
        convertBtn.Click += ConvertBtn_Click;
        convLayout.Controls.Add(convertBtn, 0, 7);

        converterPanel.Controls.Add(convLayout);

        // --- Help Content Init ---
        helpPanel = new Panel();
        helpPanel.Dock = DockStyle.Fill;
        
        helpText = new RichTextBox();
        helpText.Dock = DockStyle.Fill;
        helpText.ReadOnly = true;
        helpText.BorderStyle = BorderStyle.None;
        helpText.Font = new Font("Segoe UI", 10);
        helpText.Text = "Shortcuts:\n\n" +
                        "Basic:\n" +
                        "Digits 0-9\n" +
                        "+ - * / . \n" +
                        "Enter (=)\n" +
                        "Backspace (BS)\n" +
                        "Escape (C)\n\n" +
                        "Scientific:\n" +
                        "S = Sin(x)\n" +
                        "C = Cos(x)\n" +
                        "T = Tan(x)\n" +
                        "L = Log(x)\n" +
                        "E = Exp(x)\n" +
                        "P = PI\n" +
                        "R = Sqrt\n" +
                        "Q = Square\n\n" +
                        "Features:\n" +
                        "- Resize window for full screen\n" +
                        "- Switch Themes (Jazz!)\n" +
                        "- Export History";
        
        helpPanel.Controls.Add(helpText);
    }

    private void InitializeMainLayout()
    {
        // --- Main Calculator Area (Left) ---
        mainLayout = new TableLayoutPanel();
        mainLayout.Dock = DockStyle.Fill;
        mainLayout.RowCount = 2;
        mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 80F)); // Display Area
        mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F)); // Grid
        this.Controls.Add(mainLayout);
        mainLayout.BringToFront(); // Ensure it takes remaining space vs Right Dock

        // Display
        displayBox = new TextBox();
        displayBox.Dock = DockStyle.Fill;
        displayBox.Font = new Font("Segoe UI", 32, FontStyle.Bold); // Larger font
        displayBox.TextAlign = HorizontalAlignment.Right;
        displayBox.ReadOnly = true;
        displayBox.Text = "0";
        mainLayout.Controls.Add(displayBox, 0, 0);

        // Button Grid
        buttonGrid = new TableLayoutPanel();
        buttonGrid.Dock = DockStyle.Fill;
        buttonGrid.ColumnCount = 6;
        buttonGrid.RowCount = 6;
        for (int i = 0; i < 6; i++) {
            buttonGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 16.66F));
            buttonGrid.RowStyles.Add(new RowStyle(SizeType.Percent, 16.66F));
        }
        mainLayout.Controls.Add(buttonGrid, 0, 1);

        // Buttons
        string[] layout6 = {
            "MC", "MR", "M+", "M-", "BS", "C",
            "Sin", "Cos", "Tan", "Log", "1/x", "/",
            "π", "7", "8", "9", "*", "√",
            "Exp", "4", "5", "6", "-", "x²",
            "x^y", "1", "2", "3", "+", "%",
            "Mod", "0", ".", "DEG", "!", "="
        };

        foreach (string btnText in layout6)
        {
            Button btn = new Button();
            btn.Text = btnText;
            btn.Dock = DockStyle.Fill;
            btn.Margin = new Padding(3);
            btn.Font = new Font("Segoe UI", 12, FontStyle.Bold); // Scalable font?
            
            // Event Handling
            if (int.TryParse(btnText, out _) || btnText == ".")
            {
                btn.Click += Number_Click;
                btn.BackColor = Color.White;
                btn.Tag = "number";
            }
            else
            {
                switch (btnText)
                {
                    case "C":
                        btn.Click += Clear_Click;
                        btn.BackColor = Color.LightCoral;
                        btn.Tag = "action";
                        break;
                    case "BS":
                        btn.Click += Backspace_Click;
                        btn.BackColor = Color.Orange;
                        btn.Tag = "action";
                        break;
                    case "=":
                        btn.Click += Equals_Click;
                        btn.BackColor = Color.LightBlue;
                        btn.Tag = "action";
                        break;
                    case "DEG":
                        btn.Click += (s, e) => {
                            isDegreeMode = !isDegreeMode;
                            btn.Text = isDegreeMode ? "DEG" : "RAD";
                        };
                        btn.BackColor = Color.LightGray;
                        break;
                    case "!":
                        btn.Click += AdvancedOp_Click;
                        btn.BackColor = Color.LightGray;
                        break;
                    case "√": case "x²": case "1/x": case "%":
                    case "Sin": case "Cos": case "Tan": case "Log":
                    case "Exp": case "π":
                        btn.Click += AdvancedOp_Click;
                        btn.BackColor = Color.LightGray;
                        break;
                    case "MC": case "MR": case "M+": case "M-":
                        btn.Click += Memory_Click;
                        btn.BackColor = Color.LightGoldenrodYellow;
                        break;
                    case "Mod": case "x^y":
                        btn.Click += Operator_Click;
                        btn.BackColor = Color.LightGray;
                        break;
                    default:
                        btn.Click += Operator_Click;
                        btn.BackColor = Color.LightGray;
                        break;
                }
            }
            buttonGrid.Controls.Add(btn);
        }
    }

    private void ExportHistory_Click(object sender, EventArgs e)
    {
        SaveFileDialog saveFileDialog = new SaveFileDialog();
        saveFileDialog.Filter = "Text File|*.txt";
        saveFileDialog.Title = "Save History";
        saveFileDialog.FileName = "CalculatorHistory.txt";

        if (saveFileDialog.ShowDialog() == DialogResult.OK)
        {
            try
            {
                File.WriteAllLines(saveFileDialog.FileName, historyList.Items.Cast<string>());
                MessageBox.Show("History Exported Successfully!");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error exporting history: {ex.Message}");
            }
        }
    }

    private void ToggleSidePanel(string panelName)
    {
        sideContentPanel.Controls.Clear();
        
        Color activeColor = (themeMode == 2) ? Color.Goldenrod : Color.LightBlue;
        Color inactiveColor = (themeMode == 2) ? Color.FromArgb(60, 60, 80) : Color.LightGray;

        showHistoryBtn.BackColor = inactiveColor;
        showConverterBtn.BackColor = inactiveColor;
        showHelpBtn.BackColor = inactiveColor;

        if (panelName == "History")
        {
            sideContentPanel.Controls.Add(historyPanel);
            showHistoryBtn.BackColor = activeColor;
        }
        else if (panelName == "Converter")
        {
            sideContentPanel.Controls.Add(converterPanel);
            showConverterBtn.BackColor = activeColor;
        }
        else // Help
        {
            sideContentPanel.Controls.Add(helpPanel);
            showHelpBtn.BackColor = activeColor;
        }
    }

    private void ConversionType_Changed(object sender, EventArgs e)
    {
        fromUnit.Items.Clear();
        toUnit.Items.Clear();

        switch (conversionType.SelectedItem.ToString())
        {
            case "Length":
                string[] l = { "Meters", "Kilometers", "Miles", "Feet", "Inches" };
                fromUnit.Items.AddRange(l); toUnit.Items.AddRange(l);
                break;
            case "Weight":
                string[] w = { "Kilograms", "Pounds", "Grams", "Ounces" };
                fromUnit.Items.AddRange(w); toUnit.Items.AddRange(w);
                break;
            case "Temperature":
                string[] t = { "Celsius", "Fahrenheit", "Kelvin" };
                fromUnit.Items.AddRange(t); toUnit.Items.AddRange(t);
                break;
            case "Data":
                string[] d = { "Bytes", "Kilobytes", "Megabytes", "Gigabytes" };
                fromUnit.Items.AddRange(d); toUnit.Items.AddRange(d);
                break;
        }
        fromUnit.SelectedIndex = 0;
        toUnit.SelectedIndex = 1;
    }

    private void ConvertBtn_Click(object sender, EventArgs e)
    {
        if (double.TryParse(inputUnitBox.Text, out double val))
        {
            double result = PerformConversion(val, fromUnit.SelectedItem.ToString(), toUnit.SelectedItem.ToString(), conversionType.SelectedItem.ToString());
            outputUnitBox.Text = result.ToString("G");
        }
        else
        {
            outputUnitBox.Text = "Invalid Input";
        }
    }

    private double PerformConversion(double val, string from, string to, string type)
    {
        if (from == to) return val;

        // Simplify by converting to base unit first (m, kg, C, Byte)
        double baseVal = val;

        if (type == "Temperature")
        {
            if (from == "Celsius") baseVal = val;
            if (from == "Fahrenheit") baseVal = (val - 32) * 5/9;
            if (from == "Kelvin") baseVal = val - 273.15;

            if (to == "Celsius") return baseVal;
            if (to == "Fahrenheit") return (baseVal * 9/5) + 32;
            if (to == "Kelvin") return baseVal + 273.15;
        }
        else
        {
            // Factors to base
            double factor = 1;
            switch(from) {
                case "Kilometers": factor = 1000; break;
                case "Miles": factor = 1609.34; break;
                case "Feet": factor = 0.3048; break;
                case "Inches": factor = 0.0254; break;
                case "Pounds": factor = 0.453592; break;
                case "Grams": factor = 0.001; break;
                case "Ounces": factor = 0.0283495; break;
                case "Kilobytes": factor = 1024; break;
                case "Megabytes": factor = 1024*1024; break;
                case "Gigabytes": factor = 1024*1024*1024; break;
            }
            baseVal = val * factor;

            // Base to Target
            factor = 1;
            switch(to) {
                case "Kilometers": factor = 1000; break;
                case "Miles": factor = 1609.34; break;
                case "Feet": factor = 0.3048; break;
                case "Inches": factor = 0.0254; break;
                case "Pounds": factor = 0.453592; break;
                case "Grams": factor = 0.001; break;
                case "Ounces": factor = 0.0283495; break;
                 case "Kilobytes": factor = 1024; break;
                case "Megabytes": factor = 1024*1024; break;
                case "Gigabytes": factor = 1024*1024*1024; break;
            }
            return baseVal / factor;
        }
        return 0;
    }

    private void Theme_Click(object sender, EventArgs e)
    {
        themeMode = (themeMode + 1) % 3; // Cycle 0, 1, 2
        ApplyTheme();
    }

    private void ApplyTheme()
    {
        // 0: Light
        // 1: Dark
        // 2: Jazz (Cool Jazz Style)

        Font defaultFont = new Font("Segoe UI", 11, FontStyle.Bold);
        Font jazzFont = new Font("Century Gothic", 11, FontStyle.Bold); // Cool Jazz Font

        Color bgColor, fgColor, btnNumBg, btnOpBg, btnActionBg;

        if (themeMode == 1) // Dark
        {
            bgColor = Color.FromArgb(32, 32, 32);
            fgColor = Color.White;
            btnNumBg = Color.FromArgb(60, 60, 60);
            btnOpBg = Color.FromArgb(80, 80, 80);
            themeBtn.Text = "Dark Mode";
        }
        else if (themeMode == 2) // Jazz
        {
            bgColor = Color.FromArgb(25, 25, 45); // Deep Blue/Purple
            fgColor = Color.Gold;
            btnNumBg = Color.FromArgb(40, 40, 70);
            btnOpBg = Color.FromArgb(60, 60, 90);
            themeBtn.Text = "Jazz Mode";
        }
        else // Light
        {
            bgColor = DefaultBackColor;
            fgColor = DefaultForeColor;
            btnNumBg = Color.White;
            btnOpBg = Color.LightGray;
            themeBtn.Text = "Light Mode";
        }

        this.BackColor = bgColor;
        this.ForeColor = fgColor;
        displayBox.BackColor = (themeMode == 0) ? SystemColors.Window : (themeMode == 1 ? Color.FromArgb(45, 45, 48) : Color.FromArgb(30,30,50));
        displayBox.ForeColor = fgColor;
        displayBox.Font = (themeMode == 2) ? new Font("Century Gothic", 24, FontStyle.Bold) : new Font("Segoe UI", 24, FontStyle.Bold);

        rightDockPanel.BackColor = bgColor;
        historyList.BackColor = displayBox.BackColor;
        historyList.ForeColor = fgColor;
        historyList.Font = (themeMode == 2) ? new Font("Century Gothic", 10) : new Font("Segoe UI", 10);

        helpText.BackColor = displayBox.BackColor;
        helpText.ForeColor = fgColor;
        helpText.Font = (themeMode == 2) ? new Font("Century Gothic", 10) : new Font("Segoe UI", 10);

        themeBtn.BackColor = (themeMode == 2) ? Color.Gold : (themeMode == 1 ? Color.Gray : DefaultBackColor);
        themeBtn.ForeColor = (themeMode == 2) ? Color.Black : fgColor;

        themeBtn.ForeColor = (themeMode == 2) ? Color.Black : fgColor;
        // this.Controls.Add(themeBtn); // No need to re-add, it's in rightDockPanel

        ApplyThemeToControls(this.Controls, defaultFont, jazzFont, fgColor, btnNumBg, btnOpBg);
        
        // Refresh toggles
        if (sideContentPanel.Controls.Contains(historyPanel)) ToggleSidePanel("History");
        else if (sideContentPanel.Controls.Contains(converterPanel)) ToggleSidePanel("Converter");
        else ToggleSidePanel("Help");
    }

    private void ApplyThemeToControls(Control.ControlCollection controls, Font defaultFont, Font jazzFont, Color fgColor, Color btnNumBg, Color btnOpBg)
    {
        foreach (Control c in controls)
        {
            if (c.HasChildren)
            {
                ApplyThemeToControls(c.Controls, defaultFont, jazzFont, fgColor, btnNumBg, btnOpBg);
            }


            if (c is Button btn && btn != themeBtn && btn != showHistoryBtn && btn != showConverterBtn && btn != showHelpBtn && btn != clearHistoryBtn && btn != exportHistoryBtn && btn != convertBtn)
            {
                btn.Font = (themeMode == 2) ? jazzFont : defaultFont;
                
                if (themeMode == 0) // Light Mode Reset
                {
                    btn.ForeColor = Color.Black;
                    if (int.TryParse(btn.Text, out _) || btn.Text == ".") btn.BackColor = Color.White;
                    else if (btn.Text == "C") btn.BackColor = Color.LightCoral;
                    else if (btn.Text == "BS") btn.BackColor = Color.Orange;
                    else if (btn.Text == "=") btn.BackColor = Color.LightBlue;
                    else if (btn.Text.StartsWith("M")) btn.BackColor = Color.LightGoldenrodYellow;
                    else btn.BackColor = Color.LightGray;
                }
                else // Dark or Jazz
                {
                    btn.ForeColor = fgColor;
                    if (btn.Tag?.ToString() == "number") btn.BackColor = btnNumBg;
                    else if (btn.Name == "") 
                    {
                        if (btn.Text == "C" || btn.Text == "BS") btn.BackColor = (themeMode == 2) ? Color.OrangeRed : Color.FromArgb(100, 50, 50);
                        else if (btn.Text == "=") btn.BackColor = (themeMode == 2) ? Color.Gold : Color.FromArgb(50, 100, 150);
                        else if (btn.Text == "=" && themeMode == 2) btn.ForeColor = Color.Black;
                        else btn.BackColor = btnOpBg;
                    }
                    
                    if (themeMode == 2)
                    {
                        if (btn.Text == "=") { btn.BackColor = Color.Gold; btn.ForeColor = Color.Black; }
                         if (btn.Text == "C") { btn.BackColor = Color.Crimson; }
                    }
                }
            }
        }
    }


    private void Form1_KeyDown(object sender, KeyEventArgs e)
    {
        string key = "";
        
        // Basic Numbers & Operators
        if (e.KeyCode >= Keys.D0 && e.KeyCode <= Keys.D9 && !e.Shift) key = (e.KeyCode - Keys.D0).ToString();
        else if (e.KeyCode >= Keys.NumPad0 && e.KeyCode <= Keys.NumPad9) key = (e.KeyCode - Keys.NumPad0).ToString();
        else if (e.KeyCode == Keys.Add || (e.KeyCode == Keys.Oemplus && e.Shift)) key = "+";
        else if (e.KeyCode == Keys.Subtract || e.KeyCode == Keys.OemMinus) key = "-";
        else if (e.KeyCode == Keys.Multiply || (e.KeyCode == Keys.D8 && e.Shift)) key = "*";
        else if (e.KeyCode == Keys.Divide || e.KeyCode == Keys.OemQuestion) key = "/";
        else if (e.KeyCode == Keys.Decimal || e.KeyCode == Keys.OemPeriod) key = ".";
        else if (e.KeyCode == Keys.Enter) key = "=";
        else if (e.KeyCode == Keys.Back) key = "BS";
        else if (e.KeyCode == Keys.Escape) key = "C";
        
        // Enhanced Scientific Shortcuts
        else if (e.KeyCode == Keys.S) key = "Sin"; // S = Sin
        else if (e.KeyCode == Keys.C && !e.Control) key = "Cos"; // C = Cos
        else if (e.KeyCode == Keys.T) key = "Tan"; // T = Tan
        else if (e.KeyCode == Keys.L) key = "Log"; // L = Log
        else if (e.KeyCode == Keys.E) key = "Exp"; // E = Exp
        else if (e.KeyCode == Keys.P) key = "π";   // P = Pi
        else if (e.KeyCode == Keys.R) key = "√";   // R = Root
        else if (e.KeyCode == Keys.Q) key = "x²";  // Q = Square

        if (key != "")
        {
            PerformButtonClick(this.Controls, key);
        }
    }

    private void PerformButtonClick(Control.ControlCollection controls, string key)
    {
        foreach (Control c in controls)
        {
            if (c is Button btn && btn.Text == key)
            {
                btn.PerformClick();
                return;
            }
            if (c.HasChildren)
            {
                PerformButtonClick(c.Controls, key);
            }
        }
    }

    private void Memory_Click(object sender, EventArgs e)
    {
        Button btn = (Button)sender;
        double input = 0;
        double.TryParse(displayBox.Text, out input);

        switch (btn.Text)
        {
            case "MC":
                memoryValue = 0;
                // MessageBox.Show("Memory Cleared"); // Removed for less intrusive UI
                break;
            case "MR":
                displayBox.Text = memoryValue.ToString();
                isNewEntry = true;
                break;
            case "M+":
                memoryValue += input;
                break;
            case "M-":
                memoryValue -= input;
                break;
        }
    }

    private void ShowError(string message)
    {
        displayBox.Text = message;
        isNewEntry = true;
    }

    private void Number_Click(object sender, EventArgs e)
    {
        Button btn = (Button)sender;
        
        if (displayBox.Text == "0" || isNewEntry)
        {
            displayBox.Text = btn.Text;
            isNewEntry = false;
        }
        else
        {
            if (btn.Text == "." && displayBox.Text.Contains(".")) return;
            displayBox.Text += btn.Text;
        }
    }

    private void Operator_Click(object sender, EventArgs e)
    {
        Button btn = (Button)sender;

        // Perform previous calculation if chaining operators
        if (!isNewEntry)
        {
            Calculate();
        }

        currentOperator = btn.Text;
        double.TryParse(displayBox.Text, out currentResult);
        isNewEntry = true;
    }

    private void AdvancedOp_Click(object sender, EventArgs e)
    {
        Button btn = (Button)sender;
        double input = double.Parse(displayBox.Text);
        double result = 0;
        string opSymbol = "";

        switch (btn.Text)
        {
            case "√":
                if (input < 0) { ShowError("Invalid Input"); return; }
                result = Math.Sqrt(input);
                opSymbol = $"√({input})";
                break;
            case "x²":
                result = Math.Pow(input, 2);
                opSymbol = $"sqr({input})";
                break;
            case "1/x":
                if (input == 0) { ShowError("Divide by Zero"); return; }
                result = 1 / input;
                opSymbol = $"1/({input})";
                break;
                opSymbol = $"{input}%";
                break;
            case "!":
                if (input < 0 || input % 1 != 0) { ShowError("Invalid Input"); return; }
                if (input > 170) { ShowError("Overflow"); return; } // Factorial limit for double
                result = Factorial((int)input);
                opSymbol = $"{input}!";
                break;
            case "Sin":
                double angleS = isDegreeMode ? input * Math.PI / 180 : input;
                result = Math.Sin(angleS);
                opSymbol = isDegreeMode ? $"sin({input}°)" : $"sin({input} rad)";
                break;
            case "Cos":
                double angleC = isDegreeMode ? input * Math.PI / 180 : input;
                result = Math.Cos(angleC);
                opSymbol = isDegreeMode ? $"cos({input}°)" : $"cos({input} rad)";
                break;
            case "Tan":
                double angleT = isDegreeMode ? input * Math.PI / 180 : input;
                result = Math.Tan(angleT);
                opSymbol = isDegreeMode ? $"tan({input}°)" : $"tan({input} rad)";
                break;

            case "Log":
                if (input <= 0) { ShowError("Invalid Input"); return; }
                result = Math.Log10(input);
                opSymbol = $"log({input})";
                break;
            case "Exp": // e^x
                result = Math.Exp(input);
                opSymbol = $"e^({input})";
                break;
            case "π":
                result = Math.PI;
                opSymbol = "π";
                break;
        }

        displayBox.Text = result.ToString();
        historyList.Items.Insert(0, $"{opSymbol} = {result}");
        isNewEntry = true;
    }

    private void Backspace_Click(object sender, EventArgs e)
    {
        if (displayBox.Text.Length > 0 && !isNewEntry)
        {
            displayBox.Text = displayBox.Text.Substring(0, displayBox.Text.Length - 1);
            if (displayBox.Text == "") displayBox.Text = "0";
        }
    }

    private void Equals_Click(object sender, EventArgs e)
    {
        string opInfo = $"{currentResult} {currentOperator} {displayBox.Text}";
        Calculate();
        currentOperator = ""; // Reset operator
        isNewEntry = true; // Next input replaces result
        
        // Log to history matching the last calculation
        if (opInfo.Contains("+") || opInfo.Contains("-") || opInfo.Contains("*") || opInfo.Contains("/") || opInfo.Contains("^") || opInfo.Contains("Mod"))
        {
             historyList.Items.Insert(0, $"{opInfo} = {displayBox.Text}");
        }
    }

    private void Clear_Click(object sender, EventArgs e)
    {
        displayBox.Text = "0";
        currentResult = 0;
        currentOperator = "";
        isNewEntry = true;
    }

    private void Calculate()
    {
        if (currentOperator == "") return;

        double operand2 = 0;
        double.TryParse(displayBox.Text, out operand2);
        double result = 0;

        switch (currentOperator)
        {
            case "+":
                result = currentResult + operand2;
                break;
            case "-":
                result = currentResult - operand2;
                break;
            case "*":
                result = currentResult * operand2;
                break;
            case "/":
                if (operand2 != 0)
                    result = currentResult / operand2;
                else
                {
                    ShowError("Divide by Zero");
                    return;
                }
                break;
            case "Mod":
                result = currentResult % operand2;
                break;
            case "x^y":
                result = Math.Pow(currentResult, operand2);
                break;
        }

        displayBox.Text = result.ToString();
        currentResult = result;
    }
    private double Factorial(int n)
    {
        if (n <= 1) return 1;
        double f = 1;
        for (int i = 2; i <= n; i++) f *= i;
        return f;
    }
}
