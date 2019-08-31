import sys
import re
import json

bond_energies = {}

def LoadBonds(filename):
  bond_type = None
  symbol_row = []

  for line in open(filename, "r").readlines():

    match = re.match("\"(.*)\"", line)
    if match is not None:
      bond_type = match.group(1)
      symbol_row = []

      if not bond_type in bond_energies:
        bond_energies[bond_type] = {}

    elif bond_type is not None:
      if re.match(".*[0-9]+\\.*[0-9]*.*", line) is not None:
        symbol = re.match("([a-zA-Z]+).*", line).group(1)        

        column_index = 0
        for match in re.finditer("([0-9]+\\.*[0-9]*)", line):
          bond_energies[bond_type][(symbol, symbol_row[column_index])] = float(match.group(1))
          column_index += 1

      else:
        for match in re.finditer("([a-zA-Z]+)", line):
          symbol_row.append(match.group(1))

  for bond_type in bond_energies:
    for bond_pair in dict(bond_energies[bond_type]):
      bond_energies[bond_type][(bond_pair[1], bond_pair[0])] = bond_energies[bond_type][bond_pair];
        
      

class Bond:
  def __init__(self, type):
    self.type = type

  def Connect(self, atom0, atom1, direction):
    self.atom0 = atom0
    self.atom1 = atom1

    if direction is "Horizontal":
      atom0.east_bond = self
      atom1.west_bond = self
    else:
      atom0.south_bond = self
      atom1.north_bond = self

  def GetOtherAtom(self, atom):
    return atom1 if atom is atom0 else atom0

class Atom:
  def __init__(self, symbol):
    self.symbol = symbol
  
    self.north_bond = None
    self.east_bond = None
    self.south_bond = None
    self.west_bond = None

  def GetNorthAtom(self):
    return self.north.GetOtherAtom(self)

  def GetEastAtom(self):
    return self.east.GetOtherAtom(self)

  def GetSouthAtom(self):
    return self.south.GetOtherAtom(self)

  def GetWestAtom(self):
    return self.west.GetOtherAtom(self)


molecules = {}

def CreateMolecule(name, atoms, bonds):
  enthalpy = 0

  for bond_tuple in bonds:
    bond = bond_tuple[0]
    bond_position = bond_tuple[1]

    atom0 = None
    atom0_position = None

    atom1 = None
    atom1_position = None

    for atom_tuple in atoms:
      atom = atom_tuple[0]
      atom_position = atom_tuple[1]

      if atom_position[1] == bond_position[1]:
        if atom_position[0] < bond_position[0]:
          if atom0_position is None or bond_position[0] > atom0_position[0]:
            atom0 = atom
            atom0_position = atom_position
        elif atom1_position is None or bond_position[0] > atom1_position[0]:
          atom1 = atom
          atom1_position = atom_position

    if atom0 is None and atom1 is None:
      for atom_tuple in atoms:
        atom = atom_tuple[0]
        atom_position = atom_tuple[1]

        if atom_position[0] == bond_position[0]:
          if atom_position[1] < bond_position[1]:
            if atom0_position is None or bond_position[1] > atom0_position[1]:
              atom0 = atom
              atom0_position = atom_position
          elif atom1_position is None or bond_position[1] > atom1_position[1]:
            atom1 = atom
            atom1_position = atom_position

    if atom0 is not None and atom1 is not None:
      direction = "Horizontal" if atom0_position[1] == bond_position[1] else "Vertical"
      bond.Connect(atom0, atom1, direction)

      enthalpy -= bond_energies[bond.type][(atom0.symbol, atom1.symbol)]


  element_counts = {}
  for atom in [atom_tuple[0] for atom_tuple in atoms]:
    if atom.symbol not in element_counts:
      element_counts[atom.symbol] = 0   
    element_counts[atom.symbol] += 1

  formula = ""
  for symbol in element_counts:
    formula += str(symbol) + str(element_counts[symbol]) + " "
  formula = formula.rstrip(" ")


  molecules[name] = { "Formula" : formula, "Enthalpy" : enthalpy }

  print(name + ": " + formula)
  print("Enthalpy: " + str(enthalpy) + "\n")


def LoadMolecules(filename):
  name = None
  atoms = []
  bonds = []

  lines = open(filename, "r", encoding = "utf-16").readlines()
  line_index = -1
  for line in lines:
    line_index += 1

    match = re.match("load (.*\\.bonds) *\n", line)
    if match is not None:
      LoadBonds(match.group(1))
      continue

    match = re.match("\"(.*)\" *\n", line)
    if match is not None:
      if name is not None:
        CreateMolecule(name, atoms, bonds)

      name = match.group(1)
      atoms = []
      bonds = []

      continue

    if name is not None:
      for match in re.finditer("([^ \n]+)[ \n]", line):
        symbol = match.group(1)
        position = (match.start(1), line_index)

        if re.match("[a-zA-Z]+", symbol) is not None:
          atoms.append((Atom(match.group(1)), position))

        elif symbol == "-" or symbol == "|":
          bonds.append((Bond("Single"), position))

        elif symbol == "=" or symbol == "\u2016":
          bonds.append((Bond("Double"), position))

        elif symbol == "\u2261" or symbol == "\u2980":
          bonds.append((Bond("Triple"), position))

  if name is not None:
    CreateMolecule(name, atoms, bonds)
 

filename = sys.argv[1]

LoadMolecules(filename)

json_filepath = "Molecules/" + re.match("(.*)\\.molecules", filename).group(1) + ".json"
json_string = json.dumps({ "Molecules" : molecules }, indent = 2)
#https://stackoverflow.com/questions/46746537/json-force-every-opening-curly-brace-to-appear-in-a-new-separate-line
json_string = re.sub(r"^((\s*)\".*?\":)\s*([\[{])", r"\1\n\2\3", json_string, flags=re.MULTILINE)
json_string = re.sub("},", "},\n", json_string, flags=re.MULTILINE)

open(json_filepath, "w").write(json_string)

