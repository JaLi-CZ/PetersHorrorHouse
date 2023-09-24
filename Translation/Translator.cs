using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Translator : MonoBehaviour
{
    public static readonly string languageFileName = "language.txt";
    public static readonly string[] languageNames = new string[] { "cs", "en", "de" };
    public static readonly int languageCount = languageNames.Length, CS = 0, EN = 1, DE = 2;
    public static readonly int defaultLanguageIndex = EN;

    public static int currentLanguageIndex = GetSavedLanguage();
    public static string currentLanguage = languageNames[currentLanguageIndex];

    private static bool textsModified = false;
    private static string[][] texts = new string[][] {
    /*0 */ new string[] { "Jazyk"                      , "Language"                           , "Sprache"                                },
    /*1 */ new string[] { "Noc v Petrovì domì"         , "Night in Peter's House"             , "Nacht im Peters Haus"                   },
    /*2 */ new string[] { "Hrát"                       , "Play"                               , "Spielen"                                },
    /*3 */ new string[] { "Nastavení"                  , "Settings"                           , "Einstellungen"                          },
    /*4 */ new string[] { "Ukonèit"                    , "Exit"                               , "Beenden"                                },
    /*5 */ new string[] { "Webová Stránka"             , "Website"                            , "Webseite"                               },

    /*6 */ new string[] { "Obtížnost"                  , "Difficulty"                         , "Schwierigkeit"                          },
    /*7 */ new string[] { "Snadná"                     , "Easy"                               , "Einfach"                                },
    /*8 */ new string[] { "Støední"                    , "Medium"                             , "Mittel"                                 },
    /*9 */ new string[] { "Tìžká"                      , "Hard"                               , "Schwer"                                 },
    /*10*/ new string[] { "Extrémní"                   , "Extreme"                            , "Extrem"                                 },
    /*11*/ new string[] { "Vlastní"                    , "Custom"                             , "Benutzerdefiniert"                      },
    /*12*/ new string[] { "Rychlost Petra"             , "Peter's Speed"                      , "Peters Geschwindigkeit"                 },
    /*13*/ new string[] { "Pohyb Petra"                , "Peter's movement"                   , "Peters Bewegung"                        },
    /*14*/ new string[] { "Zcela náhodný"              , "Random"                             , "Völlig zufällig"                        },
    /*15*/ new string[] { "Podle špatného sluchu"      , "According to his bad hearing"       , "Basierend auf seinem schlechten Gehör"  },
    /*16*/ new string[] { "Podle dobrého sluchu"       , "According to his good hearing"      , "Basierend auf seinem guten Gehör"       },
    /*17*/ new string[] { "Podle výborného sluchu"     , "According to his excellent hearing" , "Basierend auf seinem exzellenten Gehör" },
    /*18*/ new string[] { "Jde pøímo za Vámi"          , "He's coming right after you"        , "Er kommt direkt hinter dir her"         },
    /*19*/ new string[] { "Viditelnost ve tmì"         , "Visibility in the dark"             , "Sichtbarkeit im Dunkeln"                },
    /*20*/ new string[] { "Dohled Petra"               , "Peter's Sight Range"                , "Peters Aufsicht"                        },
    /*21*/ new string[] { "Doba rozhlížení"            , "Peter's Idle Time"                  , "Zeit, sich umzusehen"                   },
    /*22*/ new string[] { "Poèet schovaných klíèù"     , "Hidden keys count"                  , "Versteckte Schlüssel"                   },
    /*23*/ new string[] { "Poèet potøebných klíèù"     , "Required keys count"                , "Benötigte Schlüssel"                    },
    /*24*/ new string[] { "Rychlost otáèení"           , "Sensitivity"                        , "Empfindlichkeit"                        },
    /*25*/ new string[] { "Hlasitost zvuku"            , "Sound Volume"                       , "Lautstärke"                             },
    /*26*/ new string[] { "Hlasitost hudby"            , "Music Volume"                       , "Musiklautstärke"                        },
    /*27*/ new string[] { "Tlukot srdce"               , "Heartbeat"                          , "Herzschlag"                             },
    /*28*/ new string[] { "Mluvení Petra"              , "Peter is Speaking"                  , "Peter spricht"                          },
    /*29*/ new string[] { "Problikávání baterky"       , "Flashing Flashlight"                , "Taschenlampen-Blinken"                  },
    /*30*/ new string[] { "Zrušit"                     , "Cancel"                             , "Abbrechen"                              },
    /*31*/ new string[] { "Uložit"                     , "Save"                               , "Speichern"                              },

    /*32*/ new string[] { "Naèítání..."                , "Loading..."                         , "Lädt..."                                },
    /*33*/ new string[] { "Noc I."                     , "Night I."                           , "Nacht I."                               },
    /*34*/ new string[] { "Noc II."                    , "Night II."                          , "Nacht II."                              },
    /*35*/ new string[] { "Noc III."                   , "Night III."                         , "Nacht III."                             },
    /*36*/ new string[] { "Vyhrál jsi"                 , "You Won"                            , "Du hast gewonnen"                       },
    /*37*/ new string[] { "Prohrál jsi"                , "You Lose"                           , "Du hast verloren"                       },
    /*38*/ new string[] { "Pozastaveno"                , "Game Paused"                        , "Pause"                                  },
    /*39*/ new string[] { "Pokraèovat ve høe"          , "Continue Playing"                   , "Weiterspielen"                          },
    /*40*/ new string[] { "Hlavní Menu"                , "Main Menu"                          , "Hauptmenü"                              },
    /*41*/ new string[] { "Otevøít"                    , "Open"                               , "Öffnen"                                 },
    /*42*/ new string[] { "Zavøít"                     , "Close"                              , "Schließen"                              },
    /*43*/ new string[] { "Sebrat Klíè"                , "Collect Key"                        , "Schlüssel sammeln"                      },
    /*44*/ new string[] { "Utéct z domu"               , "Finish Game"                        , "Von Zuhause weglaufen"                  },
    /*45*/ new string[] { "Nedostatek klíèù (% chybí)" , "Not Enough Keys (% missing)"        , "Nicht genügend Schlüssel (% fehlt)"     },
    
    /*46*/ new string[] { "Ovládání"                   , "Controls"                           , "Steuerung"                              },
    /*47*/ new string[] { "Pohybování"                 , "Movement"                           , "Bewegung"                               },
    /*48*/ new string[] { "Skrèení"                    , "Crouch"                             , "Ducken"                                 },
    /*49*/ new string[] { "Plná obrazovka"             , "Toggle Fullscreen"                  , "Vollbild umschalten"                    },
    /*50*/ new string[] { "Potvrdit"                   , "Confirm"                            , "Bestätigen"                             },
    /*51*/ new string[] { "Zrušit"                     , "Cancel"                             , "Abbrechen"                              },
    /*52*/ new string[] { "Vrátit se"                  , "Go Back"                            , "Zurück"                                 },

    /*53*/ new string[] { "Varování"                   , "Warning"                            , "Warnung"                                },
    /*54*/ new string[] {
               "Tato hra obsahuje explicitní scény, násilí a blikající svìtelné efekty.\n" +
               "Není vhodná pro osoby mladší 15 let a pro lidi trpící epileptickými záchvaty èi srdeèními problémy.",

               "This game contains explicit scenes, violence, and flashing light effects.\n" +
               "It is not suitable for individuals under the age of 15, those who suffer from epileptic seizures, or people with heart problems.",

               "Dieses Spiel enthält explizite Szenen, Gewalt und blinkende Lichteffekte.\n" +
               "Es ist nicht geeignet für Personen unter 15 Jahren, die an epileptischen Anfällen leiden oder Herzprobleme haben."
           },
    /*55*/ new string[] { "Pokraèovat"                 , "Continue"                           , "Weiter"                                 },
    /*56*/ new string[] { "Již nezobrazovat"           , "Don't show again"                   , "Nicht mehr anzeigen"                    },
    /*57*/ new string[] { "Nápovìda"                   , "Hint"                               , "Tip"                                    },
    /*58*/ new string[] { "Kvalita"                    , "Quality"                            , "Qualität"                               },
    /*59*/ new string[] { "Vysoká"                     , "High"                               , "Gute"                                   },
    /*60*/ new string[] { "Støední"                    , "Medium"                             , "Mittlere"                               },
    /*61*/ new string[] { "Nízká"                      , "Low"                                , "Geringe"                                },
    /*62*/ new string[] { "Pøíšerná"                   , "Terrible"                           , "Schreckliche"                           },
    /*63*/ new string[] { "Zobrazit FPS"               , "Show FPS"                           , "FPS anzeigen"                           },
    /*64*/ new string[] { "Zmìnit Hudbu"               , "Change Music"                       , "Musik ändern"                           },
    };

    public static void ModifyTexts()
    {
        if (!textsModified)
        {
            textsModified = true;
            if (GameInfo.isDeviceComputer)
            {
                for (int i = 0; i < languageCount; i++) texts[57][i] += " (H)";
            }
        }
    }

    public static string GetText(int id)
    {
        return texts[id][currentLanguageIndex];
    }

    public static void ChangeLanguage(int language)
    {
        if (language < CS || language > DE || language == currentLanguageIndex) return;
        currentLanguageIndex = language;
        currentLanguage = languageNames[language];
        Translate.UpdateAllElements();
        Voice.SwitchLanguage(currentLanguage);
        FlagSwitcher.SwitchFlag(language);
        SaveLanguage();
    }
    public static void ChangeLanguage(string language)
    {
        int languageIndex = GetLanguageIndexByName(language);
        if(languageIndex >= 0) ChangeLanguage(languageIndex);
    }

    private static int GetSavedLanguage()
    {
        string languageStr = FileManager.Read(languageFileName);
        if (languageStr == null) return defaultLanguageIndex;

        int languageIndex = GetLanguageIndexByName(languageStr);
        return languageIndex < 0 ? defaultLanguageIndex : languageIndex;
    }

    private static void SaveLanguage()
    {
        FileManager.Write(languageFileName, currentLanguage);
    }

    private static int GetLanguageIndexByName(string languageName)
    {
        int languageIndex = 0;
        foreach (string langName in languageNames)
        {
            if (langName == languageName) return languageIndex;
            languageIndex++;
        }
        return -1;
    }
}
