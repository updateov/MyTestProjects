#include "DigitalSignal.h"
#include <map>
#ifdef patterns_uses_matlab
#include "mat.h"
#endif
#include "patterns, compression.h"
#include "patterns, fetus.h"
#include "patterns, samples.h"
#include <queue>
#include <string>
#include <time.h>
#include <direct.h>

using namespace patterns;
using namespace std;

class fonction :
	public map<string, string>
{
	/*
	 -------------------------------------------------------------------------------------------------------------------
	 -------------------------------------------------------------------------------------------------------------------
	 */
	protected:
		string n;

	/*
	 -------------------------------------------------------------------------------------------------------------------
	 -------------------------------------------------------------------------------------------------------------------
	 */
	public:
		fonction(const string &);

		/*
		 ===============================================================================================================
		 ===============================================================================================================
		 */
		virtual~fonction(void)
		{
		}

		virtual string lis_nom(void) const;
		virtual string operator[](const string &) const;
		virtual bool si_connu(const string &) const;
};

class progression_console :
	public fetus::progress
{
	/*
	 -------------------------------------------------------------------------------------------------------------------
	 -------------------------------------------------------------------------------------------------------------------
	 */
	public:
		virtual void handle(void);
};

FILE *fsortie = stdout;
bool iaffiche = false, ifin = false;
map<string, fetus *> kbebes;

string en_chaine(event::type);
string en_chaine(mark::type);
void execute(const string &);
void execute_ligne(const string &);
string lis_fichier(const string &);
void quand_affiche(const fonction &);
void quand_aide(const fonction &);
void quand_ajuste_pression(const fonction &);
void quand_compare(const fonction &);
void quand_copie(const fonction &);
void quand_ecris(const fonction &);
void quand_lis(const fonction &);
fetus *quand_lis_matlab(const string &);
fetus *quand_lis_patterns(const string &);
void quand_lis_script(const string &);
void quand_oublie(const fonction &);
void quand_teste(const fonction &);
void quand_traite(const fonction &);
/**/
string rends_absolu(const string &n)
{
	return n;
}

long trouve_delta(contraction_detection *, const fetus &, long = -1, long = -1);
bool trouve_delta_si_semblables(const contraction &, const contraction &);

/*
 =======================================================================================================================
 =======================================================================================================================
 */

int main(int n, char **p)
{
	if (n > 1)
	{
		for (long i = 1; i < n; i++)
		{
			/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
			string pprefixe = string(p[i]).substr(0, 2);
			string preste = string(p[i]).substr(2);
			/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

			if (pprefixe == "-?" || pprefixe == "-h" || pprefixe == "-a")
			{
				printf
				(
					"patterns_tool [-a] [-sfichier] [fichier1 fichier2...]\n\n""   -a : affiche ce message d'aide\n""   -s : enregistre le texte de sortie dans fichier\n   \n"
					"   execute les scripts passes en parametre fichier1, fichier2, etc.\n""   lorsque l'outil est lance sans parametre, le mode interactif est\n""   utilise.\n\n"
				);
			}
			else if (pprefixe == "-s" && !preste.empty())
			{
				/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
				FILE *f = fopen(preste.c_str(), "wt");
				/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

				if (f)
				{
					fsortie = f;
				}
			}
			else
			{
				quand_lis_script(p[i]++);
			}
		}
	}
	else
	{
		/*~~~~~~~~~*/
		char t[1000];
		/*~~~~~~~~~*/

		fprintf(fsortie, "faire ? pour un sommaire des commandes.\n");
		while (!ifin)
		{
			fprintf(fsortie, "> ");
			if (gets(t))
			{
				execute(t);
			}
		}
	}

	fclose(fsortie);
	return 0;
}

/*
 =======================================================================================================================
    string en_chaine ( CFHRSignal::: BumpType k) { string r ;
    switch (k) { case CFHRSignal::: Error:: r = "Error" ;
    break ;
    case CFHRSignal::: NotAccel:: r = "NotAccel" ;
    break ;
    case CFHRSignal::: NotDecel:: r = "NotDecel" ;
    break ;
    case CFHRSignal::: NotBaseLine:: r = "NotBaseLine" ;
    break ;
    case CFHRSignal::: Acc:: r = "Acc" ;
    break ;
    case CFHRSignal::: Dec:: r = "Dec" ;
    break ;
    case CFHRSignal::: AbruptDec:: r = "AbruptDec" ;
    break ;
    case CFHRSignal::: GradDec:: r = "GradDec" ;
    break ;
    case CFHRSignal::: EarlyDec:: r = "EarlyDec" ;
    break ;
    case CFHRSignal::: LateDec:: r = "LateDec" ;
    break ;
    case CFHRSignal::: InDeterm:: r = "InDeterm" ;
    break ;
    case CFHRSignal::: InDetermAbruptDec:: r = "InDetermAbruptDec" ;
    break ;
    case CFHRSignal::: InDetermGradDec:: r = "InDetermGradDec" ;
    break ;
    case CFHRSignal::: InDetermEarlyDec:: r = "InDetermEarlyDec" ;
    break ;
    case CFHRSignal::: InDetermLateDec:: r = "InDetermLateDec" ;
    break ;
    case CFHRSignal::: UnAssoc:: r = "UnAssoc" ;
    break ;
    case CFHRSignal::: NonAssocAbruptDec:: r = "NonAssocAbruptDec" ;
    break ;
    case CFHRSignal::: NonAssocGradDec:: r = "NonAssocGradDec" ;
    break ;
    case CFHRSignal::: NonAssocEarlyDec:: r = "NonAssocEarlyDec" ;
    break ;
    case CFHRSignal::: NonAssocLateDec:: r = "NonAssocLateDec" ;
    break ;
    case CFHRSignal::: BaseLine:: r = "BaseLine" ;
    break ;
    } return r ;
    } ;
    Représentation texte d'un type d'événement.
 =======================================================================================================================
 */
string en_chaine(event::type t)
{
	/*~~~~~*/
	string r;
	/*~~~~~*/

	switch (t)
	  {
		case event::tabruptdeceleration:
			r = "abrupt deceleration";
			break;

		case event::tacceleration:
			r = "acceleration";
			break;

		case event::tbaseline:
			r = "baseline";
			break;

		case event::tdeceleration:
			r = "deceleration";
			break;

		case event::tearlydeceleration:
			r = "early deceleration";
			break;

		case event::tgradualdeceleration:
			r = "gradual deceleration";
			break;

		case event::tindeterminate:
			r = "indeterminate";
			break;

		case event::tindeterminateabruptdeceleration:
			r = "indeterminate abrupt deceleration";
			break;

		case event::tindeterminateearlydeceleration:
			r = "indeterminate early deceleration";
			break;

		case event::tindeterminategradualdeceleration:
			r = "indeterminate gradual deceleration";
			break;

		case event::tindeterminatelatedeceleration:
			r = "indeterminate late deceleration";
			break;

		case event::tlatedeceleration:
			r = "late deceleration";
			break;

		case event::tnonassociatedabruptdeceleration:
			r = "non-associated abrupt deceleration";
			break;

		case event::tnonassociatedearlydeceleration:
			r = "non-associated early deceleration";
			break;

		case event::tnonassociatedgradualdeceleration:
			r = "non-associated gradual deceleration";
			break;

		case event::tnonassociatedlatedeceleration:
			r = "non-associated late deceleration";
			break;

		case event::tnotacceleration:
			r = "not acceleration";
			break;

		case event::tnotbaseline:
			r = "not baseline";
			break;

		case event::tnotdeceleration:
			r = "not deceleration";
			break;

		case event::tunassociated:
			r = "unassociated";
			break;
	  }

	return r;
}

/*
 =======================================================================================================================
    Représentation texte d'un type de repère.
 =======================================================================================================================
 */
string en_chaine(mark::type t)
{
	/*~~~~~*/
	string r;
	/*~~~~~*/

	switch (t)
	  {
		case mark::tdiastolic:
			r = "pression diastolique maternelle";
			break;

		case mark::tgeneral:
			r = "général";
			break;

		case mark::toximetry:
			r = "oximétrie maternelle";
			break;

		case mark::tpulse:
			r = "pouls maternel";
			break;

		case mark::trespirations:
			r = "respirationsmaternelles";
			break;

		case mark::tsystolic:
			r = "pression systolique maternelle";
			break;

		case mark::ttemperature:
			r = "température maternelle";
			break;
	  }

	return r;
}

/*
 =======================================================================================================================
    Exécution de la commande donnée.
 =======================================================================================================================
 */
void execute(const string &f0)
{
	/*~~~~~~~~~~*/
	string f = f0;
	/*~~~~~~~~~~*/

	while (!f.empty())
	{
		/*~~~~~~~~~~~~~~~~~~~~~~~~~*/
		long i = (long) f.find('\n');
		/*~~~~~~~~~~~~~~~~~~~~~~~~~*/

		if (i == f.npos)
		{
			execute_ligne(f);
			f = "";
		}
		else
		{
			execute_ligne(f.substr(0, i));
			f = f.substr(i + 1);
		}

		/*
		 * else { string _a = f. substr (0, i) ;
		 * string _b = f. substr (i + 1) ;
		 * long _c = (long) f. find ('\r') ;
		 * execute_ligne (f. substr (0, i)) ;
		 * string _f = f. substr (i) ;
		 * f = f. substr (i + 1) ;
		 * }
		 */
	}
}

/*
 =======================================================================================================================
 =======================================================================================================================
 */
void execute_ligne(const string &t)
{
	/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
	fonction f = t[0] == '.' ? "test compression" : t;
	/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

	if (iaffiche)
	{
		fprintf(fsortie, "\n");
		for (map<string, string>::iterator i = f.begin(); i != f.end(); advance(i, 1))
		{
			if (i->second.empty())
			{
				fprintf(fsortie, "%s\n", i->first.c_str());
			}
			else
			{
				fprintf(fsortie, "%s : %s\n", i->first.c_str(), i->second.c_str());
			}
		}
	}

	if (f.lis_nom() == "_affiche")
	{
		iaffiche = f["_affiche"].empty() ? !iaffiche : f["_affiche"] == "oui";
	}
	else if (f.lis_nom() == "?")
	{
		quand_aide(f);
	}
	else if (f.lis_nom() == "affiche")
	{
		quand_affiche(f);
	}
	else if (f.lis_nom() == "ajuste-pression")
	{
		quand_ajuste_pression(f);
	}
	else if (f.lis_nom() == "compare")
	{
		quand_compare(f);
	}
	else if (f.lis_nom() == "copie")
	{
		quand_copie(f);
	}
	else if (f.lis_nom() == "ecris")
	{
		quand_ecris(f);
	}
	else if (f.lis_nom() == "fin")
	{
		ifin = true;
	}
	else if (f.lis_nom() == "lis")
	{
		quand_lis(f);
	}
	else if (f.lis_nom() == "oublie")
	{
		quand_oublie(f);
	}
	else if (f.lis_nom() == "teste")
	{
		quand_teste(f);
	}
	else if (f.lis_nom() == "traite")
	{
		quand_traite(f);
	}
	else
	{
		fprintf(fsortie, "commande inconnue.\n");
	}
}

/*
 =======================================================================================================================
 =======================================================================================================================
 */
fonction::fonction(const string &t)
{
	/*~~~~~~~~~~~~*/
#define note_parametre	if (!px.empty()) \
	{ \
		if (pn.empty()) \
		{ \
			insert(pair<string, string> (px, "")); \
		} \
		else \
		{ \
			insert(pair<string, string> (pn, px)); \
		} \
 \
		if (size() == 1) \
		{ \
			fonction::n = begin()->first; \
		} \
 \
		pn = px = ""; \
	}

	string pn;
	string px;
	enum { qchaine, qespace, qjeton };
	long q = qjeton;
	/*~~~~~~~~~~~~*/

	for (long i = 0, n = (long) t.length(); i < n; i++)
	{
		switch (t[i])
		  {
			case ' ':
			case '\t':
				switch (q)
				  {
					case qchaine:
						px += t[i];
						break;

					case qjeton:
						q = qespace;
						break;
				  }

				break;

			case '\"':
				if (q == qchaine)
				{
					q = qespace;
				}
				else
				{
					note_parametre;
					q = qchaine;
				}

				break;

			case ':':
				if (q != qchaine)
				{
					pn = px;
					px = "";
					q = qespace;
				}
				else
				{
					px += ':';
				}

				break;

			default:
				if (q == qespace)
				{
					note_parametre;
					q = qjeton;
				}

				px += t[i];
				break;
		  }
	}

	note_parametre;
}

/*
 =======================================================================================================================
 =======================================================================================================================
 */
string fonction::operator[](const string &n) const
{
	return si_connu(n) ? find(n)->second : "";
}

/*
 =======================================================================================================================
 =======================================================================================================================
 */
string fonction::lis_nom(void) const
{
	return n;
}

/*
 =======================================================================================================================
 =======================================================================================================================
 */
bool fonction::si_connu(const string &n) const
{
	return count(n) > 0;
}

/*
 =======================================================================================================================
    Lecture du fichier de chemin donné.
 =======================================================================================================================
 */
string lis_fichier(const string &n)
{
	/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
	FILE *f = fopen(n.c_str(), "r");
	string r;
	/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

	if (f)
	{
		/*~~~~~~~~~~~~~~~~~~~~*/
		const long nbloc = 1000;
		char t0[nbloc + 1];
		/*~~~~~~~~~~~~~~~~~~~~*/

		while (!feof(f))
		{
			/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
			long n = (long) fread(t0, sizeof(char), nbloc, f);
			/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

			r += string(t0, n);
		}

		fclose(f);
	}

	return r;
}

/*
 =======================================================================================================================
    Affichage de la progression en mode texte.
 =======================================================================================================================
 */
void progression_console::handle(void)
{
	fprintf(fsortie, "%5.1f %%\n", (double) *this * (double) 100);
}

/*
 =======================================================================================================================
    Affichage de la liste des bébés.
 =======================================================================================================================
 */
void quand_affiche(const fonction &f)
{
	/* Liste des bébés. */
	fprintf(fsortie, "variable              |fhr|   |up| |e| |c|     date\n");
	for (map < string, fetus * >::iterator i = kbebes.begin(); i != kbebes.end(); i++)
	{
		fprintf
		(
			fsortie,
			"%-20s %6ld %6ld %3ld %3ld %10ld\n",
			i->first.c_str(),
			(long) i->second->get_number_of_fhr(),
			(long) i->second->get_number_of_up(),
			(long) i->second->get_number_of_events(),
			(long) i->second->get_number_of_contractions(),
			(long long) i->second->get_start_date()
		);
	}

	/* Données et résultats pour le bébé donné. */
	if (!f[f.lis_nom()].empty())
	{
		if (kbebes.count(f[f.lis_nom()]) > 0)
		{
			/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
			const fetus &a = *kbebes[f[f.lis_nom()]];
			long i;
			long kd = 0;
			long kf = a.get_number_of_fhr() - 1;
			long n;
			bool ipoints = f.si_connu("points");
			bool inonassocies = f.si_connu("non-associes");
			/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

			fprintf(fsortie, "\n  i type                 c  début sommet    fin\n");
			if (f.si_connu("de"))
			{
				kd = atol(f["de"].c_str());
			}

			if (f.si_connu("a"))
			{
				kf = atol(f["a"].c_str());
			}

			if (kd < 0)
				kd = 0;
			if (kf >= a.get_number_of_fhr())
			{
				kf = a.get_number_of_fhr() - 1;
			}

			if (kf < 0)
				kf = 0;
			if (kd > kf)
			{
				kd = kf;
			}

			for (i = 0, n = a.get_number_of_events(); i < n; i++)
			{
				if (inonassocies || a.get_event(i).is_associated())
				{
					/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
					const event &ai = a.get_event(i);
					/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

					fprintf
					(
						fsortie,
						"%3ld %-18s %3ld %6ld %6ld %6ld\n",
						i,
						en_chaine(ai.get_type()).substr(0, 18).c_str(),
						(long) ai.get_contraction(),
						(long) ai.get_start(),
						(long) ai.get_peak(),
						(long) ai.get_end()
					);
				}
			}

			if (f.si_connu("contractions"))
			{
				fprintf(fsortie, "  i  début sommet    fin\n");
				for (i = 0, n = a.get_number_of_contractions(); i < n; i++)
				{
					/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
					const contraction &ai = a.get_contraction(i);
					/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

					fprintf(fsortie, "%3ld %6ld %6ld %6ld\n", i, (long) ai.get_start(), (long) ai.get_peak(), (long) ai.get_end());
				}
			}

			if (f.si_connu("reperes"))
			{
				fprintf(fsortie, "  i     où type       valeur description\n");
				for (i = 0, n = a.get_number_of_marks(); i < n; i++)
				{
					/*~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
					const mark &mi = a.get_mark(i);
					/*~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

					fprintf(fsortie, "%3ld %6ld %-10s %6ld %-20s\n", i, (long) mi.get_where(), en_chaine(mi.get_type()).c_str(), (long) mi.get(), mi.get_description().c_str());
				}
			}

			if (ipoints)
			{
				fprintf(fsortie, "\n     i fhr  up\n");
				for (i = kd; i < kf; i++)
				{
					fprintf(fsortie, "%6ld %3ld %3ld\n", i, (long) a.get_fhr(i), (long) a.get_up(i));
				}
			}
		}
		else
		{
			fprintf(fsortie, "variable inconnue.\n");
		}
	}
}

/*
 =======================================================================================================================
    Aide sur les différentes commandes.
 =======================================================================================================================
 */
void quand_aide(const fonction &f)
{
	/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
	bool itout = f.si_connu("tout");
	/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

	if (itout || f.size() == 1)
	{
		fprintf
		(
			fsortie,
			"? [tout] [c1] [c2]... [cn]\n""   tout           aide sur toutes les commandes\n""   c1, c2... cn   commandes connues par l'outil\n   \n"
			"   ceci affiche de l'aide sur la ou les commandes\n""   données, sur toutes les commandes si l'indicateur\n"
			"   tout est inclus ou cette aide sommaire si rien n'est\n   inclus.\n\n""affiche           affichage des fétus ou d'un fétus\n"
			"ajuste-pression   fréquence de la pression utérine\n""compare           comparaison de deux fétus\n""copie             copie partielle d'un fétus à l'autre\n"
			"ecris             exportation d'un fétus\n""lis               importation d'un fétus\n""oublie            suppression d'un fétus\n"
			"teste             lancement du test unitaire\n""traite            détection sur un fétus\n\n"
		);
	}

	if (itout || f.si_connu("affiche"))
	{
		fprintf
		(
			fsortie,
			"affiche [: b [contractions] [non-associes] [reperes] ""[points [de : d] [a : f]]]\n   : b            fétus à afficher\n""   contractions   affichage des contractions\n"
			"   reperes        affichage des repères\n""   non-associes   inclut les événements non associés\n""   points         affiche le pouls et la pression\n"
			"                  utérine\n   de : d         premier point à afficher\n""   a : f          premier point à ne pas afficher\n   \n"
			"   ceci affiche la liste des instances courantes de la\n""   classe patterns :: fetus avec leur nom, par ordre\n""   alphabétique. si b est précisé, on affiche les\n"
			"   événements détectés pour l'instance b. si non-\n""   associes est précisé, on inclut les événements non\n"
			"   associés. si points est précisé, on affiche aussi le\n""   pouls et la pression utérine. si de ou a sont\n""   précisés, on limite l'intervalle d'affiche des\n"
			"   points, sinon, on affiche tous les points.\n\n"
		);
	}

	if (itout || f.si_connu("ajuste-pression"))
	{
		fprintf
		(
			fsortie,
			"ajuste-pression : b [force]\n   : b          fétus qu'on veut ajuster\n""   force        forcer la diminution de fréquence\n   \n""   ceci garde un point sur quatre de la pression\n"
			"   utérine de l'instance donnée de fetus, faisant\n""   passer la fréquence d'échantillonnage implicite de\n""   4 Hz à 1 Hz. si force est précisé, la fréquence est\n"
			"   nécessairement diminuée, sinon, elle l'est seulement\n""   s'il y a des pouls et si le nombre de pressions est\n""   au moins la moitié du nombre de pouls. \n\n"
		);
	}

	if (itout || f.si_connu("compare"))
	{
		fprintf
		(
			fsortie,
			"compare : b1 a : b2\n   : b1     première instance à comparer\n""   a : b2   deuxième instance à comparer\n   \n""   ceci compare les deux instances de fétus données en\n"
			"   considérant les contractions, les événements\n""   détectés, les pressions utérines et les pouls. dans\n""   les deux derniers cas, si une des deux instances à\n"
			"   plus de points que l'autre mais que tous ces points\n""   sont 0 (zéro), alors ils ne sont pas considérés dans\n""   la comparaison.\n\n"
		);
	}

	if (itout || f.si_connu("copie"))
	{
		fprintf
		(
			fsortie,
			"copie : b1 a : b2 [c] [e] [fhr] [m] [up]\n   : b1     instance d'origine\n""   a : b2   instance cible\n   c        copie les contractions\n"
			"   e        copie les événements détectés\n   fhr      copie les pouls\n""   m        copie les repères\n   up       copie les pressions utérines\n"
			"   \n   ceci copie les données spécifiées de l'instance de\n""   fétus b1 vers l'instance b2, remplaçant celles de b2\n""   si b2 en contient déjà.\n\n"
		);
	}

	if (itout || f.si_connu("ecris"))
	{
		fprintf
		(
			fsortie,
			"ecris : b [dans : c] [(c++ | comprime | in-file)]\n""   : b        instance de fétus à exporter\n""   dans : c   écris dans le fichier de chemin c\n"
			"   c++        génère du code test c++\n""   comprime   écrit au format Xml comprimé\n""   in-file    écrit au format in-file\n   \n"
			"   ceci exporte l'instance de fétus b dans le fichier\n""   b.suffixe si c n'est pas précisé. si c est omis, le\n""   suffixe est déterminé en fonction du format choisi.\n"
			"   si le format n'est pas précisé, on exporte en Xml\n   non comprimé.\n\n"
		);
	}

	if (itout || f.si_connu("lis"))
	{
		fprintf
		(
			fsortie,
			"lis (: f [nom : n] | test : i)\n   : f        chemin du fichier à importer\n""   nom : n    nom à donner à l'instance créée\n""   test : i   fichier test interne à lire\n   \n"
			"   ceci crée une nouvelle instance de fétus à partir du\n""   fichier dont le chemin est f. ce fichier peut être de type .mat,\n"
			"   .xml, .in ou bien .scriptpatterns. un fichier .scriptpatterns est\n""   utilisé pour effectuer des commandes en série.\n"
			"   si n n'est pas précisé, on donne comme nom à la nouvelle instance\n""   le premier entier à partir de 0 (zéro) qui ne soit\n""   pas déjà un nom d'instance.\n\n"
		);
	}

	if (itout || f.si_connu("oublie"))
	{
		fprintf
		(
			fsortie,
			"oublie (: b | tout) [c] [e] [fhr] [up]\n""   : b    instance de fetus à oublier\n""   tout   oublie toutes les instances de fetus\n""   c      supprime les contractions\n"
			"   e      supprime les événements détectés\n   fhr    supprime les pouls\n""   up     supprime les pressions utérines\n   \n""   ceci oublie l'instance donnée b ou toutes les\n"
			"   instances si tout est précisé. par défaut, oublier\n""   signifie supprimer la ou les instances mais, si c,\n""   e, fhr ou up sont précisés, ça suppose plutôt de\n"
			"   supprimer les éléments du ou des ensembles\n   précisé.\n\n"
		);
	}

	if (itout || f.si_connu("teste"))
	{
		fprintf
		(
			fsortie,
			"teste (tout | [compression] [contractions] [fetus] [seuils])\n""   compression    test de la compression de signal\n""   contractions   test de la détection de contractions\n"
			"   fetus          test de la classe fetus\n""   seuils         seuils de détection de contractions\n""   tout           test de tous les sous-systèmes\n   \n"
			"   ceci lance les tests automatisés des différents\n""   sous-systèmes de la composante patterns et affiche\n""   les résultats. si aucun sous-système n'est précisé,\n"
			"   alors tous les sous-systèmes sont testés.\n"
		);
	}

	if (itout || f.si_connu("traite"))
	{
		fprintf(fsortie, "traite : b\n   : b   instance de fetus à traiter\n   \n""   ceci lance le traitement synchrone des données de\n""   l'instance b de fetus.\n\n");
	}
}

/*
 =======================================================================================================================
    Ajustement de l'échantillon de pression utérine. On oublie trois échantillons sur quatre pour ramener la fréquence
    d'échantillonnage de 4 Hz à 1 Hz.
 =======================================================================================================================
 */
void quand_ajuste_pression(const fonction &f)
{
	/*~~~~~~~~~~~~~~~~~~~~~~*/
	string a = f[f.lis_nom()];
	/*~~~~~~~~~~~~~~~~~~~~~~*/

	if (kbebes.count(a) > 0)
	{
		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
		bool idiminuer = f.si_connu("force");
		long m = 0;
		fetus &x = *kbebes[a];
		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

		if (f.si_connu("force") || x.get_up_sample_rate() != 1)
		{
			x.set_up_sample_rate(1);
		}
		else
		{
			fprintf(fsortie, "aucun ajustement réalisé.\n");
		}
	}
	else
	{
		fprintf(fsortie, "variable inconnue.\n");
	}
}

/*
 =======================================================================================================================
    Comparaison de deux instances.
 =======================================================================================================================
 */
void quand_compare(const fonction &f)
{
	/*~~~~~~~~~~~~~~~~~~~~~~~*/
	string a = f[f.lis_nom()];
	string b;
	bool icontractions = false;
	bool ievenements = false;
	bool ipouls = false;
	bool ipressions = false;
	bool ireperes = false;
	fetus *ka = 0;
	fetus *kb = 0;
	/*~~~~~~~~~~~~~~~~~~~~~~~*/

	/* Recherche des variables. */
	if (f.si_connu("a"))
	{
		b = f["a"];
	}

	if (kbebes.count(a) > 0)
	{
		ka = kbebes[a];
	}
	else if (a.empty())
	{
		fprintf(fsortie, "première variable manquante.\n");
	}
	else
	{
		fprintf(fsortie, "variable %s inconnue.\n", a.c_str());
	}

	if (kbebes.count(b) > 0)
	{
		kb = kbebes[b];
	}
	else if (b.empty())
	{
		fprintf(fsortie, "deuxième variable manquante.\n");
	}
	else
	{
		fprintf(fsortie, "variable %s inconnue.\n", b.c_str());
	}

	if (ka && kb)
	{
		/* Comparaison proprement dite. */
		icontractions = !ka->are_contractions_equal(*kb);
		ievenements = !ka->are_events_equal(*kb);
		ipouls = !ka->are_fhr_equal(*kb);
		ipressions = !ka->are_up_equal(*kb);
		ireperes = !ka->are_marks_equal(*kb);

		/* Affichage. */
		if (icontractions)
		{
			if (ka->contains_contractions_of(*kb))
			{
				fprintf(fsortie, "%s contient les contractions de %s.\n", a.c_str(), b.c_str());
			}
			else if (kb->contains_contractions_of(*ka))
			{
				fprintf(fsortie, "%s contient les contractions de %s.\n", b.c_str(), a.c_str());
			}
			else
			{
				fprintf(fsortie, "les contractions diffèrent.\n");
			}
		}

		if (ievenements)
		{
			if (ka->contains_events_of(*kb))
			{
				fprintf(fsortie, "%s contient les événements de %s.\n", a.c_str(), b.c_str());
			}
			else if (kb->contains_events_of(*ka))
			{
				fprintf(fsortie, "%s contient les événements de %s.\n", b.c_str(), a.c_str());
			}
			else
			{
				fprintf(fsortie, "les événements diffèrent.\n");
			}
		}

		if (ipouls)
		{
			fprintf(fsortie, "les pouls diffèrent.\n");
		}

		if (ipressions)
		{
			fprintf(fsortie, "les pressions diffèrent.\n");
		}

		if (ireperes)
		{
			fprintf(fsortie, "les repères diffèrent.\n");
		}

		if (!icontractions && !ievenements && !ipouls && !ipressions && !ireperes)
		{
			fprintf(fsortie, "les instances sont identiques.\n");
		}
	}
}

/*
 =======================================================================================================================
    Copie d'une instance à l'autre.
 =======================================================================================================================
 */
void quand_copie(const fonction &f)
{
	/*~~~~~~~~~~~~~~~~~~~~~~*/
	string a = f[f.lis_nom()];
	string b;
	fetus *ka = 0;
	fetus *kb = 0;
	/*~~~~~~~~~~~~~~~~~~~~~~*/

	/* Recherche des variables. */
	if (f.si_connu("a"))
	{
		b = f["a"];
	}

	if (kbebes.count(a) > 0)
	{
		ka = kbebes[a];
	}
	else if (a.empty())
	{
		fprintf(fsortie, "première variable manquante.\n");
	}
	else
	{
		fprintf(fsortie, "variable %s inconnue.\n", a.c_str());
	}

	if (kbebes.count(b) > 0)
	{
		kb = kbebes[b];
	}
	else if (b.empty())
	{
		fprintf(fsortie, "deuxième variable manquante.\n");
	}
	else
	{
		fprintf(fsortie, "variable %s inconnue.\n", b.c_str());
	}

	/* Copie proprement dite. */
	if (ka && kb)
	{
		if (f.si_connu("c"))
		{
			kb->reset_contractions();
			for (long i = 0, n = ka->get_number_of_contractions(); i < n; i++)
			{
				kb->append_contraction(ka->get_contraction(i));
			}
		}

		if (f.si_connu("e"))
		{
			kb->reset_events();
			for (long i = 0, n = ka->get_number_of_events(); i < n; i++)
			{
				kb->append_event(ka->get_event(i));
			}
		}

		if (f.si_connu("fhr"))
		{
			kb->reset_fhr();
			for (long i = 0, n = ka->get_number_of_fhr(); i < n; i++)
			{
				kb->append_fhr(ka->get_fhr(i));
			}
		}

		if (f.si_connu("m"))
		{
			kb->reset_marks();
			for (long i = 0, n = ka->get_number_of_marks(); i < n; i++)
			{
				kb->append_mark(ka->get_mark(i));
			}
		}

		if (f.si_connu("up"))
		{
			kb->reset_up();
			for (long i = 0, n = ka->get_number_of_up(); i < n; i++)
			{
				kb->append_up(ka->get_up(i));
			}
		}
	}
}

/*
 =======================================================================================================================
    Écriture d'une variable sur disque en Xml ou In. Si le nom du fichier cible n'est pas spécifié, on ajoute
    simplement le suffixe « .xml » ou « .in » au nom de la variable.
 =======================================================================================================================
 */
void quand_ecris(const fonction &f)
{
	/*~~~~~~~~~~~~~~~~~~~~~~*/
	string a = f[f.lis_nom()];
	string n;
	/*~~~~~~~~~~~~~~~~~~~~~~*/

	if (kbebes.count(a) > 0)
	{
		/*~~~~~~~~~~~~~~~~~~~~~~~~~~*/
		FILE *fcible;
		string s;
		fetus::format t = fetus::fxml;
		/*~~~~~~~~~~~~~~~~~~~~~~~~~~*/

		if (f.si_connu("comprime"))
		{
			t = fetus::fcompressedxml;
		}
		else if (f.si_connu("c++"))
		{
			t = fetus::fcpp;
		}
		else if (f.si_connu("in-file"))
		{
			t = fetus::fin;
		}

		switch (t)
		  {
			case fetus::fcompressedxml:
				s = ".xml";
				break;

			case fetus::fcpp:
				s = ".cpp";
				break;

			case fetus::fin:
				s = ".in";
				break;

			case fetus::fxml:
				s = ".xml";
				break;
		  }

		n = rends_absolu(f.si_connu("dans") ? f["dans"] : a + s);
		if (fcible = fopen(n.c_str(), "wb"))
		{
			/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
			string b = kbebes[a]->export(t);
			/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

			fwrite(b.c_str(), sizeof(char), b.length(), fcible);
			fclose(fcible);
		}
		else
		{
			fprintf(fsortie, "ouverture impossible.\n");
		}
	}
	else
	{
		fprintf(fsortie, "variable inconnue.\n");
	}
}

/*
 =======================================================================================================================
    Lecture d'un fichier dans une variable. On lit indifféremment les fichiers Xml et Matlab. On place les données lues
    dans une variable. Si le nom n'est pas spécifié, on donne comme nom le premier chiffre qui ne se trouve pas déjà
    dans le dictionnaire.
 =======================================================================================================================
 */
void quand_lis(const fonction &f)
{
	if (f.si_connu("test"))
	{
		kbebes.insert(pair < string, fetus * > (f["test"], samples::create(atol(f["test"].c_str()))));
	}
	else
	{
		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
		string a;
		string n = rends_absolu(f[f.lis_nom()]);
		string s;
		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

		/* Calcul du type de fichier. */
		if (n.find_last_of('.') != string::npos)
		{
			s = n.substr(n.find_last_of('.'));
		}

		/* Calcul du nom à donner à la variable. */
		if (f.si_connu("nom"))
		{
			a = f["nom"];
		}
		else
		{
			/*~~~~~~~~*/
			char ai[20];
			long i = 0;
			/*~~~~~~~~*/

			do
			{
				sprintf(ai, "%ld", i++);
			} while (kbebes.count(ai) != 0);
			a = ai;
		}

		/* Lecture du fichier. */
		{
			/*~~~~~~~~~*/
			fetus *x = 0;
			/*~~~~~~~~~*/

			if (s == ".mat")
			{
				x = quand_lis_matlab(n);
			}
			else if (s == ".xml" || s == ".in")
			{
				x = quand_lis_patterns(n);
			}
			else if (s == ".scriptpatterns")
			{
				quand_lis_script(n);
			}
			else if (!n.empty())
			{
				fprintf(fsortie, "suffixe inconnu.\n");
			}

			if (x)
			{
				kbebes.insert(pair < string, fetus * > (a, x));
			}
		}
	}
}

/*
 =======================================================================================================================
 =======================================================================================================================
 */
fetus *quand_lis_matlab(const string &n)
{
	/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
	fetus *r = new fetus;
#ifdef patterns_uses_matlab
	MATFile *f = matOpen(n.c_str(), "r");
	/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

	r->set_progress(new progression_console);
	if (f)
	{
		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
		mxArray *a = matGetVariable(f, "ud_save");
		mxArray *b = 0;
		mxArray *c = 0;
		mxArray *d = 0;
		mxArray *e = 0;
		long i;
		long n;
		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

		if (a && mxIsStruct(a))
		{
			b = mxGetField(a, 0, "fhr");
			c = mxGetField(a, 0, "uc_part_vector");
			d = mxGetField(a, 0, "uc");
			e = mxGetField(a, 0, "fhr_part_vector");
		}

		if (b && mxIsDouble(b) && mxGetNumberOfDimensions(b) > 1)
		{
			for (i = 0, n = mxGetDimensions(b)[1]; i < n; i++)
			{
				r->append_fhr((long) mxGetPr(b)[i]);
			}
		}

		if (c && mxIsStruct(c) && mxGetNumberOfDimensions(c) > 1)
		{
			for (i = 0, n = mxGetDimensions(c)[1]; i < n; i++)
			{
				/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
				long cs = 0;
				long cp = 0;
				long ce = 0;
				mxArray *e = mxGetField(c, i, "x_beg");
				mxArray *f = mxGetField(c, i, "x_peak");
				mxArray *g = mxGetField(c, i, "x_end");
				/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

				if (e && mxIsDouble(e))
				{
					cs = (long) *mxGetPr(e);
				}

				if (f && mxIsDouble(f))
				{
					cp = (long) *mxGetPr(f);
				}

				if (g && mxIsDouble(g))
				{
					ce = (long) *mxGetPr(g);
				}

				r->append_contraction(contraction(cs, cp, ce));
			}
		}

		if (d && mxIsDouble(d) && mxGetNumberOfDimensions(d) > 1)
		{
			for (i = 0, n = mxGetDimensions(d)[1]; i < n; i++)
			{
				r->append_up((long) mxGetPr(d)[i]);
			}
		}

		if (e && mxIsStruct(e) && mxGetNumberOfDimensions(e) > 1)
		{
			for (i = 0, n = mxGetDimensions(e)[1]; i < n; i++)
			{
				/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
				long cs = 0;
				long ce = 0;
				mxArray *f = mxGetField(c, i, "x_beg");
				mxArray *g = mxGetField(c, i, "x_end");
				mxArray *h = mxGetField(c, i, "part_charact");
				/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

				// mxArray *z = mxGetField (c, i, "x_end") ;
				if (f && mxIsDouble(f))
				{
					cs = (long) *mxGetPr(f);
				}

				if (g && mxIsDouble(g))
				{
					ce = (long) *mxGetPr(g);
				}

				/*~~~~~~~~~~~~~~~~~~~~~~~*/
				bool _a = h && mxIsChar(h);
				/*~~~~~~~~~~~~~~~~~~~~~~~*/

				r->append_event(event(-1, cs, -1, ce, -1, -1, (event::type) - 1));
			}
		}

		matClose(f);
	}

#endif
	return r;
}

/*
 =======================================================================================================================
 =======================================================================================================================
 */
fetus *quand_lis_patterns(const string &n)
{
	/*~~~~~~~~~~~~~~~~~*/
	fetus *r = new fetus;
	/*~~~~~~~~~~~~~~~~~*/

	r->set_progress(new progression_console);
	r->import(lis_fichier(n));
	return r;
}

/*
 =======================================================================================================================
 =======================================================================================================================
 */
void quand_lis_script(const string &n)
{
	/*~~~~~~~~~~~~~~*/
	long i;
	string c0;
	char wd[MAX_PATH];
	/*~~~~~~~~~~~~~~*/

	if (getcwd(&wd[0], MAX_PATH))
	{
		/*~~~~~~~~~~~*/
		string c1 = "";
		/*~~~~~~~~~~~*/

		for (i = (long) n.size(); i >= 0; --i)
		{
			if (n[i] == '\\')
			{
				break;
			}
		}

		if (i >= 0)
		{
			c1 = n.substr(0, i + 1);
			chdir(c1.c_str());
		}
	}

	execute(lis_fichier(n));
	chdir(wd);
}

/*
 =======================================================================================================================
    Destruction d'une variable donnée par nom.
 =======================================================================================================================
 */
void quand_oublie(const fonction &f)
{
	/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
	bool ic = f.si_connu("c");
	bool ie = f.si_connu("e");
	bool ifhr = f.si_connu("fhr");
	bool itout = f.si_connu("tout");
	bool iup = f.si_connu("up");
	/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

	if (itout && (ic || ie || ifhr || iup))
	{
		for (map < string, fetus * >::iterator i = kbebes.begin(); i != kbebes.end(); i++)
		{
			if (ic)
			{
				i->second->reset_contractions();
			}

			if (ie)
			{
				i->second->reset_events();
			}

			if (ifhr)
			{
				i->second->reset_fhr();
			}

			if (iup)
			{
				i->second->reset_up();
			}
		}
	}
	else if (itout)
	{
		while (!kbebes.empty())
		{
			delete kbebes.begin()->second;
			kbebes.erase(kbebes.begin());
		}
	}
	else if (kbebes.count(f[f.lis_nom()]) > 0 && (ic || ie || ifhr || iup))
	{
		if (ic)
		{
			kbebes[f[f.lis_nom()]]->reset_contractions();
		}

		if (ie)
		{
			kbebes[f[f.lis_nom()]]->reset_events();
		}

		if (ifhr)
		{
			kbebes[f[f.lis_nom()]]->reset_fhr();
		}

		if (iup)
		{
			kbebes[f[f.lis_nom()]]->reset_up();
		}
	}
	else if (kbebes.count(f[f.lis_nom()]) > 0)
	{
		delete kbebes[f[f.lis_nom()]];
		kbebes.erase(f[f.lis_nom()]);
	}
	else
	{
		fprintf(fsortie, "variable inconnue.\n");
	}
}

/*
 =======================================================================================================================
    Test automatisé.
 =======================================================================================================================
 */
void quand_teste(const fonction &f)
{
	/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
	bool itout = f.si_connu("tout");
	/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

	if (itout || f.si_connu("compression"))
	{
		/*~~~~~~~~~~*/
		compression a;
		/*~~~~~~~~~~*/

		fprintf(fsortie, "\ncompression...\n");
		fprintf(fsortie, "compression :: passes_test () -> %s.\n", a.passes_test() ? "vrai" : "faux");
		if (!a.get_last_test_result().empty())
		{
			fprintf(fsortie, "---\n%s---\n", a.get_last_test_result().c_str());
		}

		fprintf(fsortie, "...compression\n");
	}

	if (itout || f.si_connu("contractions"))
	{
		/*~~~~~~~~~~~~~~~~~~~~*/
		contraction_detection a;
		/*~~~~~~~~~~~~~~~~~~~~*/

		fprintf(fsortie, "\ncontractions...\n");
		fprintf(fsortie, "contraction_detection :: passes_test () -> %s.\n", a.passes_test() ? "vrai" : "faux");
		if (!a.get_last_test_result().empty())
		{
			fprintf(fsortie, "---\n%s---\n", a.get_last_test_result().c_str());
		}

		fprintf(fsortie, "...contractions\n");
	}

	if (itout || f.si_connu("fetus"))
	{
		/*~~~~*/
		fetus a;
		/*~~~~*/

		fprintf(fsortie, "\nfetus...\n");
		fprintf(fsortie, "fetus :: passes_test () -> %s.\n", a.passes_test() ? "vrai" : "faux");
		if (!a.get_last_test_result().empty())
		{
			fprintf(fsortie, "---\n%s---\n", a.get_last_test_result().c_str());
		}

		fprintf(fsortie, "...fetus\n");
	}

	if (itout || f.si_connu("seuils"))
	{
		/*~~~~~~~~~~~~~~~~~~~~*/
		contraction_detection d;
		long dminimum = 0;
		long ddroiteminimum = 0;
		/*~~~~~~~~~~~~~~~~~~~~*/

		fprintf(fsortie, "\nseuils...\n");
		d.set_latent_vector_cut_off(0.4);
		for (long i = 0; i < 6; i++)
		{
			/*~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
			long di;
			fetus *fi = samples::create(i);
			/*~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

			fi->set_up_sample_rate(1);
			for (long j = 0, nc = fi->get_number_of_contractions(); j < nc; j++)
			{
				/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
				contraction cj = fi->get_contraction(j);
				/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

				cj.set_start(cj.get_start() / 4);
				cj.set_peak(cj.get_peak() / 4);
				cj.set_end(cj.get_end() / 4);
				fi->set_contraction(j, cj);
			}

			di = trouve_delta(&d, *fi);
			dminimum = max(dminimum, di);
			delete fi;
		}

		for (long i = 0; i < 6; i++)
		{
			/*~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
			long di;
			fetus *fi = samples::create(i);
			/*~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

			fi->set_up_sample_rate(1);
			for (long j = 0, nc = fi->get_number_of_contractions(); j < nc; j++)
			{
				/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
				contraction cj = fi->get_contraction(j);
				/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

				cj.set_start(cj.get_start() / 4);
				cj.set_peak(cj.get_peak() / 4);
				cj.set_end(cj.get_end() / 4);
				fi->set_contraction(j, cj);
			}

			di = trouve_delta(&d, *fi, 300, dminimum);
			ddroiteminimum = max(ddroiteminimum, di);
			delete fi;
		}

		fprintf(fsortie, "delta minimal : %ld.\n", dminimum);
		fprintf(fsortie, "delta minimal à droite : %ld.\n", ddroiteminimum);
		fprintf(fsortie, "\n...seuils\n");
	}
}

/*
 =======================================================================================================================
    Traitement d'une instance.
 =======================================================================================================================
 */
void quand_traite(const fonction &f)
{
	if (kbebes.count(f[f.lis_nom()]) > 0)
	{
		/*~~~~~~~~~~~~~~~~~*/
		clock_t t0 = clock();
		/*~~~~~~~~~~~~~~~~~*/

		kbebes[f[f.lis_nom()]]->compute_now();
		fprintf(fsortie, "   temps de calcul : %ld s.\n", (long) ((clock() - t0) / CLOCKS_PER_SEC));
	}
	else
	{
		fprintf(fsortie, "variable inconnue.\n");
	}
}

/*
 =======================================================================================================================
 =======================================================================================================================
 */
long trouve_delta(contraction_detection *d, const fetus &f, long dgauche, long ddroite)
{
	/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
	vector<contraction> c;
	long da = 0;
	long db = f.get_number_of_up();
	long i;
	long idebut;
	long ifin;
	long j;
	long nc = f.get_number_of_contractions();
	long nu = f.get_number_of_up();
	vector<char> x;
	/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

	if (db > 500)
	{
		db = 500;
	}

	printf("--- f. get_number_of_contractions () = %ld.\n", nc);
	while (db - da > 1)
	{
		/*~~~~~~~~~~~~~~~~~~~~~*/
		bool idifference = false;
		/*~~~~~~~~~~~~~~~~~~~~~*/

		printf("    [%ld, %ld]\n", da, db);
		for (i = 0; i < nc && !idifference; i++)
		{
			/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
			const contraction &ci = f.get_contraction(i);
			/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

			if (dgauche > 0)
			{
				idebut = ci.get_start() - dgauche;
			}
			else
			{
				idebut = ci.get_start() - (da + db) / 2;
			}

			if (ddroite > 0)
			{
				ifin = ci.get_end() + ddroite;
			}
			else
			{
				ifin = ci.get_end() + (da + db) / 2;
			}

			if (idebut < 0)
			{
				idebut = 0;
			}

			if (ifin >= nu)
			{
				ifin = nu - 1;
			}

			x.resize(ifin - idebut + 1);
			for (j = idebut; j <= ifin; j++)
			{
				x[j - idebut] = (char) (unsigned char) f.get_up(j);
			}

			c = d->detect(x);

			/*
			 * printf ("| idébut = %ld.\n", idebut) ;
			 * printf ("| ifin = %ld.\n", ifin) ;
			 * printf ("| |c| = %ld.\n", (long) c. size ()) ;
			 * printf ("| ci. get_start () = %ld.\n", ci. get_start ()) ;
			 * printf ("| ci. get_peak () = %ld.\n", ci. get_peak ()) ;
			 * printf ("| ci. get_end () = %ld.\n", ci. get_end ()) ;
			 */
			idifference = true;
			for (j = 0; j < (long) c.size() && idifference; j++)
			{
				c[j].set_start(c[j].get_start() + idebut);
				c[j].set_peak(c[j].get_peak() + idebut);
				c[j].set_end(c[j].get_end() + idebut);
				idifference = !trouve_delta_si_semblables(ci, c[j]);
			}

			if (idifference)
			{
				printf("    --- (%ld, %ld, %ld) dans [%ld, %ld].\n", ci.get_start(), ci.get_peak(), ci.get_end(), idebut, ifin);
			}

			/*
			 * for (j = 0 ;
			 * j < (long) c. size () ;
			 * j++) { printf ("| c [%ld]. get_start () = %ld.\n", j, c [j]. get_start ()) ;
			 * printf ("| c [%ld]. get_peak () = %ld.\n", j, c [j]. get_peak ()) ;
			 * printf ("| c [%ld]. get_end () = %ld.\n", j, c [j]. get_end ()) ;
			 * } printf ("| idifférence = %s.\n", idifference ? "vrai" : "faux") ;
			 */
		}

		if (idifference)
		{
			da = (da + db) / 2;
		}
		else
		{
			db = (da + db) / 2;
		}
	}

	return db;
}

/*
 =======================================================================================================================
 =======================================================================================================================
 */
bool trouve_delta_si_semblables(const contraction &c1, const contraction &c2)
{
	/*~~~~~~~~~~~~~~~~~*/
	long negaux = 0;
	long nsemblables = 0;
	/*~~~~~~~~~~~~~~~~~*/

	if (c1.get_start() == c2.get_start())
	{
		negaux++;
	}

	if (c1.get_peak() == c2.get_peak())
	{
		negaux++;
	}

	if (c1.get_end() == c2.get_end())
	{
		negaux++;
	}

	if (abs(c1.get_start() - c2.get_start()) < 10)
	{
		nsemblables++;
	}

	if (abs(c1.get_peak() - c2.get_peak()) < 10)
	{
		nsemblables++;
	}

	if (abs(c1.get_end() - c2.get_end()) < 10)
	{
		nsemblables++;
	}

	return negaux > 1 && negaux + nsemblables >= 3;
}
