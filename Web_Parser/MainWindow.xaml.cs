using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using AngleSharp;
using AngleSharp.Dom;
using AngleSharp.Html.Parser;
using AngleSharp.Io;
using System.Text.RegularExpressions;
using System.Net;
using System.IO;
using AngleSharp.Text;
using System.Diagnostics.Eventing.Reader;
using System.Windows.Media;
using System.Windows.Controls.Primitives;

namespace Web_Parser
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        readonly string[] LBI = { "link_save.txt", "result_save.txt", "info_save.txt" };
        readonly string[] LBI_last = { "last_link_save.txt", "last_result_save.txt", "last_info_save.txt" };
        public MainWindow()
        {
            InitializeComponent();
            Output_Parametrs_Method(LBI); // LBI - link, box, info TEXT
        }
        private void ParsButton_Click(object sender, RoutedEventArgs e) //standart pars web-site
        {
            string selector = "style, script";
            string value = string.Empty;
            Parser_Method(selector, value); //use method programm pars with parametr selector "style, script"
 
            InfoTable_Method(); //use method output in InfoTab

            Save_Parametrs_Method(LBI); //use method save parametrs link, text, info in TxtFile
        }

        private void Info_Click(object sender, RoutedEventArgs e) //open / close InfoTab & create yourself pars
        {        
            var style = (Style)Resources["ButtonStyle_InfoPage"];
            if (Info.Style == style)
            {
                Info.Style = (Style)Resources["ButtonStyle_InfoBackPage"];
                Width = 1540;
            }
            else
            {
                Info.Style = style;
                Width = 800;
            }
        }

        private void My_ParsChange_Click(object sender, RoutedEventArgs e) //individual settings panel
        {
            Save_Parametrs_Method(LBI_last);

            if (WebLink_Box.Text != string.Empty)
            {
                switch (((Button)sender).TabIndex)
                {
                    case 1:
                        var result = Regex.Replace(ParsText_Box.Text, RegexChange1.Text, RegexChange2.Text);
                        ParsText_Box.Text = result;
                        break;
                    case 2:
                        var Client = new WebClient();
                        var WebText = Client.DownloadString(WebLink_Box.Text);
                        WebText = Regex.Replace(WebText, RegularPars_Box.Text, "");
                        ParsText_Box.Text = WebText;
                        break;
                    case 3:
                        var config = Configuration.Default.WithDefaultLoader();
                        var context = BrowsingContext.New(config);
                        var document = context.OpenAsync(WebLink_Box.Text).Result;

                        if (AddContent_Check.IsChecked == false)
                            ParsText_Box.Clear();

                        var selectorAll = document.QuerySelectorAll(SelectorAllPars_Box.Text);
                        int count;
                        if (Count_Selector.Text == "All")
                            count = selectorAll.Length;
                        else
                            count = Convert.ToInt32(Count_Selector.Text); 

                            if (AttributeAllPars_Box.Text == "TextContent")
                        {
                            for(int i = 0; i < count; i++)
                                ParsText_Box.Text += selectorAll[i].TextContent + "\n";
                        }
                        else
                        {
                            for (int i = 0; i < count; i++)
                                ParsText_Box.Text += selectorAll[i].GetAttribute(AttributeAllPars_Box.Text) + "\n";
                        }
                        break;
                }
                InfoTable_Method();

                Save_Parametrs_Method(LBI);
            }
        }
        
        private void Back_Result_Click(object sender, RoutedEventArgs e)
        {
            Output_Parametrs_Method(LBI_last);

            Save_Parametrs_Method(LBI_last);
        }

        private void ParsText_Box_Scroll(object sender, ScrollEventArgs e) //scrolling result pars text
        {
            scrollBar.Maximum = ParsText_Box.LineCount - 1;
            ParsText_Box.ScrollToLine(Convert.ToInt32(scrollBar.Value));
        }

        private void ParsText_ScrollWheel(object sender, System.Windows.Input.MouseWheelEventArgs e)
        {
            // Пустое событие для того, чтобы скроллить можно было кликом мыши???
            //Сам не понял если честно
        }

        void InfoTable_Method()
        { // method output in InfoTab
            int probels = 0, number = 0;
            char[] ParsArr = ParsText_Box.Text.ToCharArray();
            for (int i = 0; i < ParsArr.Length; i++)
            {
                if (ParsArr[i] == ' ')
                    probels++;
                if (ParsArr[i].IsDigit())
                    number++;
            }
            Info1.Content = "Количество символов: " + ParsText_Box.Text.Length;
            Info2.Content = $"Количество численных символов: {number}";
            Info3.Content = $"Количество буквенных символов: {ParsText_Box.Text.Length - (probels + number)}";
            Info4.Content = $"Количество пробелов: {probels}";
        }
        void Save_Parametrs_Method(string[] LBI)
        {
            List<Label> info_line = new List<Label>() { Info0, Info1, Info2, Info3, Info4 };

            using (StreamWriter write = new StreamWriter(@"TextFile\" + LBI[0]))
                write.Write(WebLink_Box.Text);
            using (StreamWriter write = new StreamWriter(@"TextFile\" + LBI[1]))
                write.Write(ParsText_Box.Text);

            using (StreamWriter write = new StreamWriter(@"TextFile\" + LBI[2]))
                for (int i = 0; i < info_line.Count; i++)
                    write.WriteLine(info_line[i].Content);
        }
        void Output_Parametrs_Method(string[] LBI)
        {
            WebLink_Box.Text = File.ReadAllText(@"TextFile\" + LBI[0]);
            ParsText_Box.Text = File.ReadAllText(@"TextFile\" + LBI[1]);

            List<Label> info_line = new List<Label>() { Info0, Info1, Info2, Info3, Info4 };
            string[] lineFromTxt = File.ReadAllLines(@"TextFile\" + LBI[2]);
            for (int i = 0; i < info_line.Count; i++)
                info_line[i].Content = lineFromTxt[i];
        }
        void Parser_Method(string selector, string value)
        {
            ParsText_Box.Clear();

            var config = Configuration.Default.WithDefaultLoader();
            var context = BrowsingContext.New(config);
            var document = context.OpenAsync(WebLink_Box.Text).Result;

            if (WebLink_Box.Text != string.Empty)
            {
                var result = document.QuerySelectorAll(selector);
                foreach (var s in result)
                    s.InnerHtml = value;

                result = document.QuerySelectorAll(" * ");
                foreach (var s in result)
                    ParsText_Box.Text += s.TextContent + " ";

                var end = Regex.Replace(ParsText_Box.Text, @"\s+", " ");
                ParsText_Box.Text = end;

                if (document.Title == string.Empty)
                    Info0.Content = $"Данные с сайта отсутствуют";
                else
                    Info0.Content = $"Данные с сайта \"{document.Title}\"";
            }
        }
    }
}