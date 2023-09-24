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
    /*1 */ new string[] { "Noc v Petrov� dom�"         , "Night in Peter's House"             , "Nacht im Peters Haus"                   },
    /*2 */ new string[] { "Hr�t"                       , "Play"                               , "Spielen"                                },
    /*3 */ new string[] { "Nastaven�"                  , "Settings"                           , "Einstellungen"                          },
    /*4 */ new string[] { "Ukon�it"                    , "Exit"                               , "Beenden"                                },
    /*5 */ new string[] { "Webov� Str�nka"             , "Website"                            , "Webseite"                               },

    /*6 */ new string[] { "Obt�nost"                  , "Difficulty"                         , "Schwierigkeit"                          },
    /*7 */ new string[] { "Snadn�"                     , "Easy"                               , "Einfach"                                },
    /*8 */ new string[] { "St�edn�"                    , "Medium"                             , "Mittel"                                 },
    /*9 */ new string[] { "T�k�"                      , "Hard"                               , "Schwer"                                 },
    /*10*/ new string[] { "Extr�mn�"                   , "Extreme"                            , "Extrem"                                 },
    /*11*/ new string[] { "Vlastn�"                    , "Custom"                             , "Benutzerdefiniert"                      },
    /*12*/ new string[] { "Rychlost Petra"             , "Peter's Speed"                      , "Peters Geschwindigkeit"                 },
    /*13*/ new string[] { "Pohyb Petra"                , "Peter's movement"                   , "Peters Bewegung"                        },
    /*14*/ new string[] { "Zcela n�hodn�"              , "Random"                             , "V�llig zuf�llig"                        },
    /*15*/ new string[] { "Podle �patn�ho sluchu"      , "According to his bad hearing"       , "Basierend auf seinem schlechten Geh�r"  },
    /*16*/ new string[] { "Podle dobr�ho sluchu"       , "According to his good hearing"      , "Basierend auf seinem guten Geh�r"       },
    /*17*/ new string[] { "Podle v�born�ho sluchu"     , "According to his excellent hearing" , "Basierend auf seinem exzellenten Geh�r" },
    /*18*/ new string[] { "Jde p��mo za V�mi"          , "He's coming right after you"        , "Er kommt direkt hinter dir her"         },
    /*19*/ new string[] { "Viditelnost ve tm�"         , "Visibility in the dark"             , "Sichtbarkeit im Dunkeln"                },
    /*20*/ new string[] { "Dohled Petra"               , "Peter's Sight Range"                , "Peters Aufsicht"                        },
    /*21*/ new string[] { "Doba rozhl�en�"            , "Peter's Idle Time"                  , "Zeit, sich umzusehen"                   },
    /*22*/ new string[] { "Po�et schovan�ch kl���"     , "Hidden keys count"                  , "Versteckte Schl�ssel"                   },
    /*23*/ new string[] { "Po�et pot�ebn�ch kl���"     , "Required keys count"                , "Ben�tigte Schl�ssel"                    },
    /*24*/ new string[] { "Rychlost ot��en�"           , "Sensitivity"                        , "Empfindlichkeit"                        },
    /*25*/ new string[] { "Hlasitost zvuku"            , "Sound Volume"                       , "Lautst�rke"                             },
    /*26*/ new string[] { "Hlasitost hudby"            , "Music Volume"                       , "Musiklautst�rke"                        },
    /*27*/ new string[] { "Tlukot srdce"               , "Heartbeat"                          , "Herzschlag"                             },
    /*28*/ new string[] { "Mluven� Petra"              , "Peter is Speaking"                  , "Peter spricht"                          },
    /*29*/ new string[] { "Problik�v�n� baterky"       , "Flashing Flashlight"                , "Taschenlampen-Blinken"                  },
    /*30*/ new string[] { "Zru�it"                     , "Cancel"                             , "Abbrechen"                              },
    /*31*/ new string[] { "Ulo�it"                     , "Save"                               , "Speichern"                              },

    /*32*/ new string[] { "Na��t�n�..."                , "Loading..."                         , "L�dt..."                                },
    /*33*/ new string[] { "Noc I."                     , "Night I."                           , "Nacht I."                               },
    /*34*/ new string[] { "Noc II."                    , "Night II."                          , "Nacht II."                              },
    /*35*/ new string[] { "Noc III."                   , "Night III."                         , "Nacht III."                             },
    /*36*/ new string[] { "Vyhr�l jsi"                 , "You Won"                            , "Du hast gewonnen"                       },
    /*37*/ new string[] { "Prohr�l jsi"                , "You Lose"                           , "Du hast verloren"                       },
    /*38*/ new string[] { "Pozastaveno"                , "Game Paused"                        , "Pause"                                  },
    /*39*/ new string[] { "Pokra�ovat ve h�e"          , "Continue Playing"                   , "Weiterspielen"                          },
    /*40*/ new string[] { "Hlavn� Menu"                , "Main Menu"                          , "Hauptmen�"                              },
    /*41*/ new string[] { "Otev��t"                    , "Open"                               , "�ffnen"                                 },
    /*42*/ new string[] { "Zav��t"                     , "Close"                              , "Schlie�en"                              },
    /*43*/ new string[] { "Sebrat Kl��"                , "Collect Key"                        , "Schl�ssel sammeln"                      },
    /*44*/ new string[] { "Ut�ct z domu"               , "Finish Game"                        , "Von Zuhause weglaufen"                  },
    /*45*/ new string[] { "Nedostatek kl��� (% chyb�)" , "Not Enough Keys (% missing)"        , "Nicht gen�gend Schl�ssel (% fehlt)"     },
    
    /*46*/ new string[] { "Ovl�d�n�"                   , "Controls"                           , "Steuerung"                              },
    /*47*/ new string[] { "Pohybov�n�"                 , "Movement"                           , "Bewegung"                               },
    /*48*/ new string[] { "Skr�en�"                    , "Crouch"                             , "Ducken"                                 },
    /*49*/ new string[] { "Pln� obrazovka"             , "Toggle Fullscreen"                  , "Vollbild umschalten"                    },
    /*50*/ new string[] { "Potvrdit"                   , "Confirm"                            , "Best�tigen"                             },
    /*51*/ new string[] { "Zru�it"                     , "Cancel"                             , "Abbrechen"                              },
    /*52*/ new string[] { "Vr�tit se"                  , "Go Back"                            , "Zur�ck"                                 },

    /*53*/ new string[] { "Varov�n�"                   , "Warning"                            , "Warnung"                                },
    /*54*/ new string[] {
               "Tato hra obsahuje explicitn� sc�ny, n�sil� a blikaj�c� sv�teln� efekty.\n" +
               "Nen� vhodn� pro osoby mlad�� 15 let a pro lidi trp�c� epileptick�mi z�chvaty �i srde�n�mi probl�my.",

               "This game contains explicit scenes, violence, and flashing light effects.\n" +
               "It is not suitable for individuals under the age of 15, those who suffer from epileptic seizures, or people with heart problems.",

               "Dieses Spiel enth�lt explizite Szenen, Gewalt und blinkende Lichteffekte.\n" +
               "Es ist nicht geeignet f�r Personen unter 15 Jahren, die an epileptischen Anf�llen leiden oder Herzprobleme haben."
           },
    /*55*/ new string[] { "Pokra�ovat"                 , "Continue"                           , "Weiter"                                 },
    /*56*/ new string[] { "Ji� nezobrazovat"           , "Don't show again"                   , "Nicht mehr anzeigen"                    },
    /*57*/ new string[] { "N�pov�da"                   , "Hint"                               , "Tip"                                    },
    /*58*/ new string[] { "Kvalita"                    , "Quality"                            , "Qualit�t"                               },
    /*59*/ new string[] { "Vysok�"                     , "High"                               , "Gute"                                   },
    /*60*/ new string[] { "St�edn�"                    , "Medium"                             , "Mittlere"                               },
    /*61*/ new string[] { "N�zk�"                      , "Low"                                , "Geringe"                                },
    /*62*/ new string[] { "P��ern�"                   , "Terrible"                           , "Schreckliche"                           },
    /*63*/ new string[] { "Zobrazit FPS"               , "Show FPS"                           , "FPS anzeigen"                           },
    /*64*/ new string[] { "Zm�nit Hudbu"               , "Change Music"                       , "Musik �ndern"                           },
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
