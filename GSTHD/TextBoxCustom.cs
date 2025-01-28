using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using static System.Net.Mime.MediaTypeNames;

namespace GSTHD
{
    class TextBoxCustom : Control
    {
        private Settings settings;
        public TextBox TextBoxField;
        public ListBox SuggestionContainer;
        Dictionary<string, string> ListSuggestion;
        public Dictionary<string, string> KeycodesWithTag;
        bool SuggestionContainerIsFocus = false;
        public string codestring = string.Empty;
        public string panelstring = string.Empty;
        private bool isPath = false;
        public bool isErrorMessage = false;
        private HintPanelType hintPanelType;
        public List<MixedSubPanels> ListSubs;

        public TextBoxCustom(Settings _settings, Dictionary<string, string> listSuggestion, Point location, Color color, Font font, string name, Size size, string text, HintPanelType ptype, bool _isPath=false, Dictionary<string, string> kpt=null)
        {
            ListSuggestion = listSuggestion;

            settings = _settings;
            isPath = _isPath;
            hintPanelType = ptype;
            KeycodesWithTag = kpt;

            TextBoxField = new TextBox
            {
                Location = new Point(location.X, location.Y),
                BorderStyle = BorderStyle.None,
                BackColor = color,
                CausesValidation = false,
                Font = font,
                ForeColor = Color.White,
                Name = name,
                AutoSize = false,
                Size = size,
                Text = text,
                AcceptsTab = true
            };
            TextBoxField.KeyUp += TextBoxCustom_KeyUp;
            TextBoxField.PreviewKeyDown += TextBoxField_PreviewKeyDown;
            TextBoxField.KeyDown += TextBoxField_KeyDown;
            TextBoxField.LostFocus += TextBoxField_LostFocus;
            TextBoxField.SelectionStart = TextBoxField.TextLength;

            SuggestionContainer = new ListBox
            {
                Visible = false,
                Location = new Point(TextBoxField.Location.X, TextBoxField.Location.Y + TextBoxField.Height + 5),
                IntegralHeight = false,
                Width = 155,
                Sorted = false
            };
            SuggestionContainer.Items.AddRange(listSuggestion.Keys.ToArray());
            SuggestionContainer.KeyUp += SuggestionContainer_KeyUp;
        }

        private void TextBoxField_LostFocus(object sender, EventArgs e)
        {
            //Weird but works ¯\_(ツ)_/¯
            if (SuggestionContainerIsFocus) SuggestionContainer.Hide();
        }

        private void TextBoxField_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Tab)
            {
                e.Handled = true;
                e.SuppressKeyPress = true;
            }

            if(e.Control && e.KeyCode == Keys.R)
            {
                e.Handled = true;
                e.SuppressKeyPress = true;
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


        private void TextBoxField_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            if (e.KeyCode == Keys.Tab && !isErrorMessage)
            {
                e.IsInputKey = true;
                if (SuggestionContainer.Items.Count > 0)
                {
                    SuggestionContainer.SelectedIndex = 0;
                    TextBoxField.Text = SuggestionContainer.Text;
                    SuggestionContainer.Hide();
                    SuggestionContainer.Items.Clear();
                }
            } else if (e.KeyCode == Keys.Enter && !isErrorMessage)
            {
                var textbox = (TextBox)sender;

                if (!SuggestionContainer.Items.Contains(textbox.Text) && SuggestionContainer.Items.Count > 0)
                {
                    textbox.Lines = new string[3] { panelstring, codestring, SuggestionContainer.Items[0].ToString() };
                } else
                {
                    if (panelstring != string.Empty && codestring != string.Empty)
                    {
                        string[] tempsplit = textbox.Text.Split(new char[] { ' ' }, count: 3);
                        if (tempsplit.Length > 2)
                        {
                            textbox.Lines = new string[3] { panelstring, codestring, tempsplit[2] };
                        }
                        else
                        {
                            textbox.Lines = new string[3] { panelstring, codestring, "" };
                        }
                    } else if (codestring != string.Empty || panelstring != string.Empty)
                    {
                        string[] tempsplit = textbox.Text.Split(new char[] { ' ' }, count: 2);
                        if (tempsplit.Length > 1)
                        {
                            textbox.Lines = new string[3] { panelstring, codestring, tempsplit[1] };
                        } else
                        {
                            textbox.Lines = new string[3] { panelstring, codestring, "" };
                        }
                    } else
                    {
                        textbox.Lines = new string[3] { "", "", textbox.Text };
                    }
                }
            } else if (isErrorMessage)
            {
                ((TextBox)sender).Text = string.Empty;
                isErrorMessage = false;
            }
        }

        private void SuggestionContainer_KeyUp(object sender, KeyEventArgs e)
        {
            if(e.KeyCode == Keys.Enter)
            {
                var fulltext = TextBoxField.Text.Split(new char[] { ' ' }, count: 3);
                if (settings.HintPathAutofill)
                {
                    TextBoxField.Text = AttachTexts(SuggestionContainer.SelectedItem.ToString());
                }
                else
                {
                    TextBoxField.Text = SuggestionContainer.SelectedItem.ToString();
                }
                TextBoxField.Focus();
                SuggestionContainer.Hide();
                SuggestionContainerIsFocus = false;
            }
        }

        private void TextBoxCustom_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Down)
            {
                if (SuggestionContainer.Items.Count > 0)
                {
                    SuggestionContainer.Focus();
                    SuggestionContainer.SelectedIndex = 0;
                    SuggestionContainerIsFocus = true;
                }
            }
            else if(e.KeyCode == Keys.Tab)
            {
                //Do Nothing
            }
            else 
            {
                var textbox = (TextBox)sender;
                SuggestionContainer.Items.Clear();

                string vartext = ParseCodesAndText(textbox.Text.ToLower());
                if (vartext == "")
                {
                    SuggestionContainer.Hide();
                    return;
                }

                var listTagFiltered = ListSuggestion.Where(x => x.Value.Contains(vartext)).Select(y => y.Key);
                SuggestionContainer.Items.AddRange(listTagFiltered.ToArray());

                var listPlacesFiltered = ListSuggestion.Keys.Where(x => x.ToLower().Contains(vartext));
                foreach (var element in listPlacesFiltered.ToArray())
                {
                    if (!SuggestionContainer.Items.Contains(element))
                        SuggestionContainer.Items.Add(element);
                }
                if (TextBoxField.Text.Length > 0 && SuggestionContainer.Items.Count > 0) SuggestionContainer.Show();
                else SuggestionContainer.Hide();
            }
        }

        public void SetSuggestionsContainerLocation(Point location)
        {
            SuggestionContainer.Location = new Point
            (
                location.X + TextBoxField.Location.X,
                location.Y + TextBoxField.Location.Y + TextBoxField.Height
            );
        }

        public void newLocation(Point newLocation, Point panelLocation)
        {
            TextBoxField.Location = newLocation;
            TextBoxField.Multiline = false;
            SetSuggestionsContainerLocation(panelLocation);
        }

    }
}
