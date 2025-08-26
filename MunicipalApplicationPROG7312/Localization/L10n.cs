using System;                          // StringComparison
using System.Collections.Generic;      // Dictionary, IEnumerable, LinkedList (for categories if needed)

namespace MunicipalApplicationPROG7312.Localization
{
    // Simple DTO to bind to ComboBox
    public sealed class LangOption
    {
        public string Code { get; }     // "en", "af", "xh", "zu"
        public string Name { get; }     // "English", "Afrikaans", ...
        public LangOption(string code, string name) { Code = code; Name = name; }
        public override string ToString() => Name; // what shows in ComboBox
    }

    public static class L10n
    {
        // Current language (this property fixes your compile error)
        public static string CurrentLanguageCode { get; private set; } = "en";

        // Minimal string store. Add the rest of your keys here (same keys across languages).
        private static readonly Dictionary<string, Dictionary<string, string>> _strings =
            new Dictionary<string, Dictionary<string, string>>(StringComparer.OrdinalIgnoreCase)
            {
                ["en"] = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                {
                    ["Main_Title"] = "Municipal Reporter",
                    ["Btn_Report"] = "Report issues",
                    ["Btn_Events"] = "Local events & announcements (coming soon)",
                    ["Btn_Status"] = "Service request status (coming soon)",
                    ["Lbl_Location"] = "Location",
                    ["Lbl_Category"] = "Category",
                    ["Lbl_Description"] = "Description",
                    ["Lbl_Attachments"] = "Attachments",
                    ["Btn_AddFile"] = "Add image or document",
                    ["Btn_Back"] = "Back to Main Menu",
                    ["Consent_Text"] = "I consent to use of my data to process this report only",

                    // Categories (used by Categories() below)
                    ["Cat_Sanitation"] = "Sanitation",
                    ["Cat_Roads"] = "Roads",
                    ["Cat_Utilities"] = "Utilities",
                    ["Cat_Waste"] = "Waste",
                    ["Cat_Other"] = "Other",

                    // Submitted dialog
                    ["Submitted_Header"] = "Your report was submitted.",
                    ["Submitted_Ticket"] = "Ticket:",
                    ["Submitted_Status"] = "Status:",
                    ["Status_New"] = "New",
                    ["Submitted_Target"] = "Target response: 48h for sanitation, 72h for roads/utilities.",
                    ["Submitted_Thanks"] = "Thank you for reporting.",
                    ["Submitted_Title"] = "Submitted",
                    ["Btn_RemoveFile"] = "Remove selected",
                    ["Msg_SelectFileToRemove"] = "Select one or more attachments to remove.",
                    ["Title_Remove"] = "Remove attachment",

                    // Hints / errors
                    ["Hint_0"] = "Start by adding location",
                    ["Hint_1"] = "Good – now select a category",
                    ["Hint_2"] = "Add a short description",
                    ["Hint_3"] = "Optional – attach a photo or document",
                    ["Hint_4"] = "Last step – tick consent and submit",
                    ["Err_MissingLocation"] = "Please enter the location.",
                    ["Title_MissingLocation"] = "Missing location",
                    ["Err_MissingCategory"] = "Please select a category.",
                    ["Title_MissingCategory"] = "Missing category",
                    ["Err_MissingDescription"] = "Please add a short description.",
                    ["Title_MissingDescription"] = "Missing description",
                    ["Err_ConsentNeeded"] = "Please provide consent to process your report.",
                    ["Title_Consent"] = "Consent needed",
                    ["FD_Title"] = "Select images or documents",
                    ["FD_Filter"] = "Images and Documents|*.png;*.jpg;*.jpeg;*.gif;*.pdf;*.doc;*.docx;*.txt",
                    ["Err_SubmitGeneric"] = "We could not submit your report."
                },

                // Add real translations as you progress.
                ["af"] = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                {
                    ["Main_Title"] = "Munisipale Verslaggewer",
                    ["Btn_Report"] = "Rapporteer probleme",
                    ["Btn_Events"] = "Plaaslike gebeure en aankondigings (binnekort)",
                    ["Btn_Status"] = "Diensversoekstatus (binnekort)",
                    ["Lbl_Location"] = "Ligging",
                    ["Lbl_Category"] = "Kategorie",
                    ["Lbl_Description"] = "Beskrywing",
                    ["Lbl_Attachments"] = "Aanhangsels",
                    ["Btn_AddFile"] = "Voeg beeld of dokument by",
                    ["Btn_Back"] = "Terug na hoofkieslys",
                    ["Consent_Text"] = "Ek stem in tot die gebruik van my data om slegs hierdie verslag te verwerk",
                    ["Cat_Sanitation"] = "Sanitasie",
                    ["Cat_Roads"] = "Paaie",
                    ["Cat_Utilities"] = "Nutdienste",
                    ["Cat_Waste"] = "Afval",
                    ["Cat_Other"] = "Ander",
                    ["Submitted_Header"] = "Jou verslag is ingedien.",
                    ["Submitted_Ticket"] = "Kaartjie:",
                    ["Submitted_Status"] = "Status:",
                    ["Status_New"] = "Nuut",
                    ["Submitted_Target"] = "Teikenreaksie: 48h vir sanitasie, 72h vir paaie/nutdienste.",
                    ["Submitted_Thanks"] = "Dankie vir jou verslag.",
                    ["Submitted_Title"] = "Ingedien",
                    ["Hint_0"] = "Begin met die ligging",
                    ["Hint_1"] = "Kies nou 'n kategorie",
                    ["Hint_2"] = "Voeg 'n kort beskrywing by",
                    ["Hint_3"] = "Opsioneel – heg 'n foto of dokument aan",
                    ["Hint_4"] = "Laaste stap – merk toestemming en dien in",
                    ["Err_MissingLocation"] = "Voer asseblief die ligging in.",
                    ["Title_MissingLocation"] = "Ligging ontbreek",
                    ["Err_MissingCategory"] = "Kies asseblief 'n kategorie.",
                    ["Title_MissingCategory"] = "Kategorie ontbreek",
                    ["Err_MissingDescription"] = "Voeg asseblief 'n kort beskrywing by.",
                    ["Title_MissingDescription"] = "Beskrywing ontbreek",
                    ["Err_ConsentNeeded"] = "Verskaf asseblief toestemming om jou verslag te verwerk.",
                    ["Title_Consent"] = "Toestemming nodig",
                    ["FD_Title"] = "Kies beelde of dokumente",
                    ["FD_Filter"] = "Beelde en Dokumente|*.png;*.jpg;*.jpeg;*.gif;*.pdf;*.doc;*.docx;*.txt",
                    ["Err_SubmitGeneric"] = "Ons kon nie jou verslag indien nie.",
                    ["Btn_RemoveFile"] = "Verwyder gekies",
                    ["Msg_SelectFileToRemove"] = "Kies een of meer aanhangsels om te verwyder.",
                    ["Title_Remove"] = "Verwyder aanhangsel",
                },

                ["xh"] = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                {
                    ["Main_Title"] = "Umbikishi weMasipala",
                    ["Btn_Report"] = "Xela ingxaki",
                    ["Btn_Events"] = "Iziganeko neZaziso (iya kufika)",
                    ["Btn_Status"] = "Isimo sesicelo senkonzo (iya kufika)",
                    ["Lbl_Location"] = "Indawo",
                    ["Lbl_Category"] = "Udidi",
                    ["Lbl_Description"] = "Inkcazo",
                    ["Lbl_Attachments"] = "Izincamathiselo",
                    ["Btn_AddFile"] = "Fakela umfanekiso okanye uxwebhu",
                    ["Btn_Back"] = "Buyela kwiMenyu engundoqo",
                    ["Consent_Text"] = "Ndiyayivuma into yokuba idatha yam isetyenziselwe olu xwebhu kuphela",
                    ["Cat_Sanitation"] = "Ucoceko",
                    ["Cat_Roads"] = "Iindlela",
                    ["Cat_Utilities"] = "Iinkonzo",
                    ["Cat_Waste"] = "Inkunkuma",
                    ["Cat_Other"] = "Okunye",
                    ["Submitted_Header"] = "Ingxelo yakho ithunyeliwe.",
                    ["Submitted_Ticket"] = "Ithekhethi:",
                    ["Submitted_Status"] = "Isimo:",
                    ["Status_New"] = "Entsha",
                    ["Submitted_Target"] = "Ixesha ekujoliswe kulo: iiyure ezingama-48 zococeko, 72 zeendlela/inkonzo.",
                    ["Submitted_Thanks"] = "Enkosi ngokuxela.",
                    ["Submitted_Title"] = "KuThunyelwe",
                    ["Hint_0"] = "Qala ngendawo",
                    ["Hint_1"] = "Khetha udidi",
                    ["Hint_2"] = "Nika inkcazo emfutshane",
                    ["Hint_3"] = "Ongakhethiyo – faka umfanekiso okanye uxwebhu",
                    ["Hint_4"] = "Inyathelo lokugqibela – khangela imvume uze ungenise",
                    ["Err_MissingLocation"] = "Nceda ngenisa indawo.",
                    ["Title_MissingLocation"] = "Indawo ayikho",
                    ["Err_MissingCategory"] = "Nceda khetha udidi.",
                    ["Title_MissingCategory"] = "Udidi alukho",
                    ["Err_MissingDescription"] = "Nceda nika inkcazo emfutshane.",
                    ["Title_MissingDescription"] = "Inkcazo ayikho",
                    ["Err_ConsentNeeded"] = "Nceda unike imvume yokusebenza kwengxelo yakho.",
                    ["Title_Consent"] = "Imvume iyafuneka",
                    ["FD_Title"] = "Khetha imifanekiso okanye amaxwebhu",
                    ["FD_Filter"] = "Imifanekiso neMaxwebhu|*.png;*.jpg;*.jpeg;*.gif;*.pdf;*.doc;*.docx;*.txt",
                    ["Err_SubmitGeneric"] = "Asikwazanga ukuthumela ingxelo yakho.",
                    ["Btn_RemoveFile"] = "Susa okukhethiwe",
                    ["Msg_SelectFileToRemove"] = "Khetha ifayile enye okanye ezininzi ezinamathiselweyo ukuze uzisuse.",
                    ["Title_Remove"] = "Susa unamathiselo",
                },

                ["zu"] = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                {
                    ["Main_Title"] = "Umbikishi Wedolobha",
                    ["Btn_Report"] = "Bika inkinga",
                    ["Btn_Events"] = "Imicimbi nezaziso (kuyalandela)",
                    ["Btn_Status"] = "Isimo sesicelo (kuyalandela)",
                    ["Lbl_Location"] = "Indawo",
                    ["Lbl_Category"] = "Isigaba",
                    ["Lbl_Description"] = "Incazelo",
                    ["Lbl_Attachments"] = "Izithasiselo",
                    ["Btn_AddFile"] = "Nezela isithombe noma umbhalo",
                    ["Btn_Back"] = "Buyela kwimenyu enkulu",
                    ["Consent_Text"] = "Ngiyanikeza imvume yokuthi idatha yami isetshenziselwe lolu biko kuphela",
                    ["Cat_Sanitation"] = "Uhlanzeko",
                    ["Cat_Roads"] = "Imigwaqo",
                    ["Cat_Utilities"] = "Izinsiza",
                    ["Cat_Waste"] = "Udoti",
                    ["Cat_Other"] = "Okunye",
                    ["Submitted_Header"] = "Umbiko wakho uthunyelwe.",
                    ["Submitted_Ticket"] = "Ithikithi:",
                    ["Submitted_Status"] = "Isimo:",
                    ["Status_New"] = "Okusha",
                    ["Submitted_Target"] = "Isikhathi okuhlosiwe: amahora angu-48 kohlanzeko, 72 emigwaqweni/ezinsizeni.",
                    ["Submitted_Thanks"] = "Ngiyabonga ngokubika.",
                    ["Submitted_Title"] = "Kuthunyelwe",
                    ["Hint_0"] = "Qala ngendawo",
                    ["Hint_1"] = "Khetha isigaba",
                    ["Hint_2"] = "Faka incazelo emfushane",
                    ["Hint_3"] = "Ongakukhetha – faka isithombe noma umbhalo",
                    ["Hint_4"] = "Isinyathelo sokugcina – makha imvume bese uthumela",
                    ["Err_MissingLocation"] = "Sicela ufake indawo.",
                    ["Title_MissingLocation"] = "Indawo ayikho",
                    ["Err_MissingCategory"] = "Sicela ukhethe isigaba.",
                    ["Title_MissingCategory"] = "Isigaba asikho",
                    ["Err_MissingDescription"] = "Sicela ufake incazelo emfushane.",
                    ["Title_MissingDescription"] = "Incazelo ayikho",
                    ["Err_ConsentNeeded"] = "Sicela unike imvume yokucubungula umbiko wakho.",
                    ["Title_Consent"] = "Imvume iyadingeka",
                    ["FD_Title"] = "Khetha izithombe noma imibhalo",
                    ["FD_Filter"] = "Izithombe Nemibhalo|*.png;*.jpg;*.jpeg;*.gif;*.pdf;*.doc;*.docx;*.txt",
                    ["Err_SubmitGeneric"] = "Asikwazanga ukuthumela umbiko wakho.",
                    ["Btn_RemoveFile"] = "Susa okukhethiwe",
                    ["Msg_SelectFileToRemove"] = "Khetha isinamathiselo esisodwa noma eziningi ukuze uzisuse.",
                    ["Title_Remove"] = "Susa isinamathiselo",
                }
            };

        // Translate a key for the current language (falls back to key if missing)
        public static string T(string key)
        {
            if (_strings.TryGetValue(CurrentLanguageCode, out var lang) && lang.TryGetValue(key, out var val))
                return val;
            return key;
        }

        // Change language at runtime
        public static void SetLanguage(string code)
        {
            if (!_strings.ContainsKey(code)) code = "en";
            CurrentLanguageCode = code;
        }

        // Language options for the ComboBox (yield to avoid arrays/lists)
        public static IEnumerable<LangOption> LanguageOptions()
        {
            yield return new LangOption("en", "English");
            yield return new LangOption("af", "Afrikaans");
            yield return new LangOption("xh", "isiXhosa");
            yield return new LangOption("zu", "isiZulu");
        }

        // Categories as an iterator (UI loops and adds items)
        public static IEnumerable<string> Categories()
        {
            yield return T("Cat_Sanitation");
            yield return T("Cat_Roads");
            yield return T("Cat_Utilities");
            yield return T("Cat_Waste");
            yield return T("Cat_Other");
        }
    }
}