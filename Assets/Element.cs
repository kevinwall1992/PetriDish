using System.Collections.Generic;


public class Element
{
    public static Dictionary<string, Element> elements = new Dictionary<string, Element>();

    static Element()
    {
        elements["H"] = new Element("Hydrogen", 1);
        elements["He"] = new Element("Helium", 2);
        elements["Li"] = new Element("Lithium", 3);
        elements["Be"] = new Element("Beryllium", 4);
        elements["B"] = new Element("Boron", 5);
        elements["C"] = new Element("Carbon", 6);
        elements["N"] = new Element("Nitrogen", 7);
        elements["O"] = new Element("Oxygen", 8);
        elements["F"] = new Element("Flourine", 9);
        elements["Ne"] = new Element("Neon", 10);
        elements["Na"] = new Element("Sodium", 11);
        elements["Mg"] = new Element("Magnesium", 12);
        elements["Al"] = new Element("Aluminium", 13);
        elements["Si"] = new Element("Silicon", 14);
        elements["P"] = new Element("Phosphorus", 15);
        elements["S"] = new Element("Sulfur", 16);
        elements["Cl"] = new Element("Chlorine", 17);
        elements["Ar"] = new Element("Argon", 18);
        elements["K"] = new Element("Potassium", 19);
        elements["Ca"] = new Element("Calcium", 20);
        elements["Sc"] = new Element("Scandium", 21);
        elements["Ti"] = new Element("Titanium", 22);
        elements["V"] = new Element("Vanadium", 23);
        elements["Cr"] = new Element("Chromium", 24);
        elements["Mn"] = new Element("Manganese", 25);
        elements["Fe"] = new Element("Iron", 26);
        elements["Co"] = new Element("CobaLT", 27);
        elements["Ni"] = new Element("Nickel", 28);
        elements["Cu"] = new Element("Copper", 29);
        elements["Zn"] = new Element("Zinc", 30);
        elements["Ga"] = new Element("Gallium", 31);
        elements["Ge"] = new Element("Germanium", 32);
        elements["As"] = new Element("Arsenic", 33);
        elements["Se"] = new Element("Selenium", 34);
        elements["Br"] = new Element("Bromine", 35);
        elements["Kr"] = new Element("Krypton", 36);
        elements["Rb"] = new Element("Rubidium", 37);
        elements["Sr"] = new Element("Strontium", 38);
        elements["Y"] = new Element("Yttrium", 39);
        elements["Zr"] = new Element("Zirconium", 40);
        elements["Nb"] = new Element("Niobium", 41);
        elements["Mo"] = new Element("Molybdenum", 42);
        elements["Tc"] = new Element("Technetium", 43);
        elements["Ru"] = new Element("Ruthenium", 44);
        elements["Rh"] = new Element("Rhodium", 45);
        elements["Pd"] = new Element("Palladium", 46);
        elements["Ag"] = new Element("Silver", 47);
        elements["Cd"] = new Element("Cadmium", 48);
        elements["In"] = new Element("Indium", 49);
        elements["Sn"] = new Element("Tin", 50);
        elements["Sb"] = new Element("Antimony", 51);
        elements["Te"] = new Element("Tellurium", 52);
        elements["I"] = new Element("Iodine", 53);
        elements["Xe"] = new Element("Xenon", 54);
        elements["Cs"] = new Element("Caesium", 55);
        elements["Ba"] = new Element("Barium", 56);
        elements["La"] = new Element("Lanthanum", 57);
        elements["Ce"] = new Element("Cerium", 58);
        elements["Pr"] = new Element("Praseodymium", 59);
        elements["Nd"] = new Element("Neodymium", 60);
        elements["Pm"] = new Element("Promethium", 61);
        elements["Sm"] = new Element("Samarium", 62);
        elements["Eu"] = new Element("Europium", 63);
        elements["Gd"] = new Element("Gadolinium", 64);
        elements["Tb"] = new Element("Terbium", 65);
        elements["Dy"] = new Element("Dysprosium", 66);
        elements["Ho"] = new Element("Holmium", 67);
        elements["Er"] = new Element("Erbium", 68);
        elements["Tm"] = new Element("Thulium", 69);
        elements["Yb"] = new Element("Ytterbium", 70);
        elements["Lu"] = new Element("Lutetium", 71);
        elements["Hf"] = new Element("Hafnium", 72);
        elements["Ta"] = new Element("Tantalum", 73);
        elements["W"] = new Element("Tungsten", 74);
        elements["Re"] = new Element("Rhemium", 75);
        elements["Os"] = new Element("Osmium", 76);
        elements["Ir"] = new Element("Iridium", 77);
        elements["Pt"] = new Element("Platinum", 78);
        elements["Au"] = new Element("Gold", 79);
        elements["Hg"] = new Element("Mercury", 80);
        elements["Tl"] = new Element("Thallium", 81);
        elements["Pb"] = new Element("Lead", 82);
        elements["Bi"] = new Element("Bismuth", 83);
        elements["Po"] = new Element("Polonium", 84);
        elements["At"] = new Element("Astatine", 85);
        elements["Rn"] = new Element("Radon", 86);
        elements["Fr"] = new Element("Francium", 87);
        elements["Ra"] = new Element("Radium", 88);
        elements["Ac"] = new Element("Actinium", 89);
        elements["Th"] = new Element("Thorium", 90);
        elements["Pa"] = new Element("Protactinium", 91);
        elements["U"] = new Element("Uranium", 92);
        elements["Np"] = new Element("Neptunium", 93);
        elements["Pu"] = new Element("Plutonium", 94);
        elements["Am"] = new Element("Americium", 95);
        elements["Cm"] = new Element("Curium", 96);
        elements["Bk"] = new Element("Berkelium", 97);
        elements["Cf"] = new Element("Californium", 98);
        elements["Es"] = new Element("Einsteinium", 99);
        elements["Fm"] = new Element("Fermium", 100);
        elements["Md"] = new Element("Mendelevium", 101);
        elements["No"] = new Element("Nobelium", 102);
        elements["Lr"] = new Element("Lawrencium", 103);
        elements["Rf"] = new Element("Rutherfordium", 104);
        elements["Db"] = new Element("Dubnium", 105);
        elements["Sg"] = new Element("Seaborgium", 106);
        elements["Bh"] = new Element("Bohrium", 107);
        elements["Hs"] = new Element("Hasium", 108);
        elements["Mt"] = new Element("Meitnerium", 109);
        elements["Ds"] = new Element("Darmstadtium", 110);
        elements["Rg"] = new Element("Roentgenium", 111);
        elements["Cn"] = new Element("Copernicium", 112);
        elements["Nh"] = new Element("Nihonium", 113);
        elements["Fl"] = new Element("Flerovium", 114);
        elements["Mc"] = new Element("Moscovium", 115);
        elements["Lv"] = new Element("Livermorium", 116);
        elements["Ts"] = new Element("Tennessine", 117);
        elements["Og"] = new Element("Ogenesson", 118);
    }

    string name;
    int atomic_number;

    public string Name
    {
        get { return name; }
    }

    public int AtomicNumber
    {
        get { return atomic_number; }
    }

    //simplification, not sure its matters though
    public int Mass
    {
        get { return atomic_number * 2; }
    }

    public Element(string name_, int atomic_number_)
    {
        name = name_;
        atomic_number = atomic_number_;
    }
}
