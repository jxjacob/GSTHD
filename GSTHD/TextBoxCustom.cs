using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Color = System.Drawing.Color;
using Point = System.Drawing.Point;
using Size = System.Drawing.Size;

namespace GSTHD
{
    
    class TextBoxCustom : TextBox
    {
        protected override Type StyleKeyOverride => typeof(TextBox);
        
        private Settings settings;
        public ListBox SuggestionContainer;
        Dictionary<string, string> ListSuggestion;
        public Dictionary<string, string> KeycodesWithTag;
        public bool SuggestionContainerIsFocus = false;
        public string codestring = string.Empty;
        public string panelstring = string.Empty;
        private bool isPath = false;
        public bool isErrorMessage = false;
        private HintPanelType hintPanelType;
        public List<MixedSubPanels> ListSubs;
        public string[] Lines;

        public TextBoxCustom(Settings _settings, Dictionary<string, string> listSuggestion, Point location, Color color, FontData font, string name, Size size, string text, HintPanelType ptype, bool _isPath=false, Dictionary<string, string> kpt=null)
        {
            ListSuggestion = listSuggestion;

            settings = _settings;
            isPath = _isPath;
            hintPanelType = ptype;
            KeycodesWithTag = kpt;

            BorderThickness = new Thickness(0);
            Padding = new Thickness(0);
            Margin = new Thickness(0);
            Background = new SolidColorBrush(new Avalonia.Media.Color(color.A,  color.R, color.G, color.B));
            // CausesValidation = false,
            FontFamily = font.fontName;
            FontSize = font.fontSize * 1.4;
            FontStyleFix(font.fontStyle);
            Foreground = new SolidColorBrush(Avalonia.Media.Color.FromRgb(255,255,255));
            Name = name;
            // AutoSize = false,
            Width = size.Width;
            Height = size.Height;
            MinHeight = 0;
            MinWidth = 0;
            CornerRadius = new CornerRadius(0);
            Watermark = text;
            // AcceptsTab = true;

            Canvas.SetTop(this, location.Y);
            Canvas.SetLeft(this, location.X);
            this.KeyDown += TextBoxField_PreviewKeyDown;
            this.KeyDown += TextBoxField_KeyDown;
            LostFocus += TextBoxField_LostFocus;
            KeyUp += TextBoxCustom_KeyUp;
            
            
            SuggestionContainer = new ListBox
            {
                IsVisible = false,
                // Location = new Point(TextBoxField.Location.X, TextBoxField.Location.Y + TextBoxField.Height + 5),
                // IntegralHeight = false,
                Width = size.Width,
                MaxHeight = 300,
                Margin = new Thickness(0)
            };
            Canvas.SetTop(SuggestionContainer, location.Y + Height + 5);
            Canvas.SetLeft(SuggestionContainer, location.X);
            SuggestionContainer.ItemsSource = listSuggestion.ToArray();
            SuggestionContainer.KeyUp += SuggestionContainer_KeyUp;
        }
        
        private void TextBoxField_LostFocus(object sender, EventArgs e)
        {
            SuggestionContainer.IsVisible = false;
        }
        
        private void TextBoxField_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Tab)
            {
                e.Handled = true;
                // e.SuppressKeyPress = true;
            }

            if(e.KeyModifiers == KeyModifiers.Control && e.Key == Key.R)
            {
                e.Handled = true;
                // e.SuppressKeyPress = true;
            }
        }

        private string ParseCodesAndText(string senttext)
        {
            panelstring = string.Empty;
            codestring = string.Empty;
            MixedSubPanels currentsub = null;
            var reftext = senttext.Split(new char[] { ' ' });
            if (hintPanelType == HintPanelType.Mixed)
            {
                foreach (var sub in ListSubs)
                {
                    if (sub.Keycode == reftext[0])
                    {
                        panelstring = reftext[0];
                        currentsub = sub;
                    }
                }
                if (panelstring == string.Empty)
                {
                    // not a valid code, just send it back tbh
                    return senttext;
                } else
                {
                    if (reftext.Length > 1)
                    {
                        // valid code, got what we're looking for, moving on
                        reftext = reftext.Skip(1).ToArray();
                    } else
                    {
                        // valid code, but no text yet so just keep the faith and send back nothing
                        return "";
                    }
                }
            }

            if (reftext[0] == "") return "";

            bool tempispath = (currentsub != null) ? (currentsub.PathGoalCount > 0 || currentsub.OuterPathID != null) : isPath;
            bool tempisquan = (currentsub != null) ? (currentsub.CounterFontSize != 0) : (hintPanelType == HintPanelType.Quantity);

            if (tempispath && settings.HintPathAutofill)
            {
                Dictionary<string, string> FoundKeycodes = new Dictionary<string, string> { };
                // if theres a letter that isnt a code, bail
                foreach (char x in reftext[0])
                {
                    if (KeycodesWithTag.ContainsKey(x.ToString()))
                    {
                        if (!FoundKeycodes.ContainsKey(x.ToString())) FoundKeycodes.Add(x.ToString(), KeycodesWithTag[x.ToString()]);
                    }
                    else if (!settings.HintPathAutofillAggressive)
                    {
                        FoundKeycodes.Clear();
                        break;
                    }
                }

                // if there is no lookup, then assume the code is a misinterpit and add it back
                if (FoundKeycodes.Count > 0)
                {
                    codestring = reftext[0];
                    reftext = reftext.Skip(1).ToArray();
                }
            } else if (tempisquan && settings.HintPathAutofill)
            {
                int foundin = 0;
                try
                {
                    foundin = int.Parse(reftext[0]);
                }
                catch
                {

                }

                if (foundin != 0)
                {
                    codestring = reftext[0];
                    reftext = reftext.Skip(1).ToArray();
                }
            }


            // recombine whats left and prepate to ship 'er
            //string temptemp = string.Join(" ", reftext);
            //Debug.WriteLine("tt="+temptemp);
            return string.Join(" ", reftext);

            // normal panels need to just return the input
            // path panels + thesetting need to parse the first string for codes and seperate them if theyre valid

            // mixed panels need to segment out the first piece for codes, and then act like one of the other ones while also keeping track of that first code
            // and if that first code dont work then just ~~fucking crash who cares idk~~ send a different error code and skip the rest of the processing

            // and then EVENTUALLY, needs to send back to the woth panel the subcode, keycodes, and plaintext
            // unless theres an autosuggestion, then its that instead of the plaintext

            // returns the text that should be used as reference for the suggestionbox
            
        }

        private string AttachTexts(string inp)
        {
            string tempstring = string.Empty;
            if (panelstring != string.Empty) tempstring = panelstring;
            if (codestring != string.Empty)
            {
                if (tempstring != string.Empty)
                {
                    tempstring += " " + codestring;
                } else
                {
                    tempstring = codestring;
                }
            }
            if (tempstring == string.Empty)
            {
                tempstring = inp;
            } else
            {
                tempstring += " " + inp;
            }
            return tempstring;
        }


        private void TextBoxField_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Tab && !isErrorMessage)
            {
                // e.IsInputKey = true;
                if (SuggestionContainer.Items.Count > 0)
                {
                    SuggestionContainer.SelectedIndex = 0;
                    // Text = SuggestionContainer.Text;
                    SuggestionContainer.IsVisible = false;
                    SuggestionContainer.Items.Clear();
                }
            } else if (e.Key == Key.Enter && !isErrorMessage)
            {
                var textbox = (TextBoxCustom)sender;
            
                if (!SuggestionContainer.Items.Contains(textbox.Text) && SuggestionContainer.Items.Count > 0)
                {
                    textbox.Lines = [panelstring, codestring, SuggestionContainer.Items[0].ToString()];
                } else
                {
                    if (panelstring != string.Empty && codestring != string.Empty)
                    {
                        string[] tempsplit = textbox.Text.Split([' '], count: 3);
                        if (tempsplit.Length > 2)
                        {
                            textbox.Lines = [panelstring, codestring, tempsplit[2]];
                        }
                        else
                        {
                            textbox.Lines = [panelstring, codestring, ""];
                        }
                    } else if (codestring != string.Empty || panelstring != string.Empty)
                    {
                        string[] tempsplit = textbox.Text.Split([' '], count: 2);
                        if (tempsplit.Length > 1)
                        {
                            textbox.Lines = [panelstring, codestring, tempsplit[1]];
                        } else
                        {
                            textbox.Lines = [panelstring, codestring, ""];
                        }
                    } else
                    {
                        textbox.Lines = ["", "", textbox.Text];
                    }
                }
            } else if (isErrorMessage)
            {
                ((TextBoxCustom)sender).Text = string.Empty;
                isErrorMessage = false;
            }
        }

        private void SuggestionContainer_KeyUp(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.Enter)
            {
                var fulltext = Text.Split(new char[] { ' ' }, count: 3);
                if (settings.HintPathAutofill)
                {
                    Text = AttachTexts(SuggestionContainer.SelectedItem.ToString());
                }
                else
                {
                    Text = SuggestionContainer.SelectedItem.ToString();
                }
                Focus();
                SuggestionContainer.IsVisible = false;
                SuggestionContainerIsFocus = false;
            }
        }
        
        private void TextBoxCustom_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Down)
            {
                if (SuggestionContainer.Items.Count > 0)
                {
                    SuggestionContainer.Focus();
                    SuggestionContainer.SelectedIndex = 0;
                    SuggestionContainerIsFocus = true;
                }
            }
            else if(e.Key == Key.Tab)
            {
                //Do Nothing
            }
            else 
            {
                var textbox = (TextBox)sender;

                string vartext = ParseCodesAndText(textbox.Text.ToLower());
                if (vartext == "")
                {
                    SuggestionContainer.IsVisible = false;
                    return;
                }

                IEnumerable<string> listTagFiltered = ListSuggestion.Where(x => x.Value.Contains(vartext)).Select(y => y.Key);
                IEnumerable<string> listPlacesFiltered = ListSuggestion.Keys.Where(x => x.Contains(vartext, StringComparison.CurrentCultureIgnoreCase));
                if (!listTagFiltered.Any())
                {
                    listTagFiltered = listPlacesFiltered;
                }
                else
                {
                    foreach (var element in listPlacesFiltered.ToArray())
                    {
                        if (!listTagFiltered.Contains(element))
                        {
                            listTagFiltered.Append(element);
                        }
                    }
                }
                SuggestionContainer.ItemsSource = listTagFiltered.ToArray();

                if (Text.Length > 0 && SuggestionContainer.Items.Count > 0) SuggestionContainer.IsVisible = true;
                else SuggestionContainer.IsVisible = false;
            }
        }
        
        public void SetSuggestionsContainerLocation(Point location)
        {
            Canvas.SetTop(SuggestionContainer, location.Y + Canvas.GetTop(this) + Height);
            Canvas.SetLeft(SuggestionContainer, location.X + Canvas.GetLeft(this));
        }

        public void newLocation(Point newLocation, Point panelLocation)
        {
            Canvas.SetTop(this, newLocation.Y);
            Canvas.SetLeft(this, newLocation.X);
            SetSuggestionsContainerLocation(panelLocation);
        }
        
        
        private void FontStyleFix(FontStyle style)
        {
            // sucks that this is manual but what you gonna do?
            switch (style)
            {
                case GSTHD.FontStyle.Bold:
                    FontWeight = FontWeight.Bold;
                    FontStyle = Avalonia.Media.FontStyle.Normal;
                    break;
                case GSTHD.FontStyle.Italic:
                    FontWeight = FontWeight.Normal;
                    FontStyle = Avalonia.Media.FontStyle.Italic;
                    break;
                case GSTHD.FontStyle.Regular:
                    FontWeight = FontWeight.Normal;
                    FontStyle = Avalonia.Media.FontStyle.Normal;
                    break;
            }
        }
    }
}
