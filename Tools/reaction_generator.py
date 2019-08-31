import sys
import re
import json

molecules = {}

def LoadMolecules(filename):
  molecules_json = json.load(open(filename))
  
  for molecule_name in molecules_json["Molecules"]:
    molecules[molecule_name] = {}

    for match in re.finditer("([a-zA-Z]+)([0-9]*)", molecules_json["Molecules"][molecule_name]["Formula"]):
      element_symbol = match.group(1)
      element_quantity = int(match.group(2)) if not match.group() == "" else 1

      molecules[molecule_name][element_symbol] = element_quantity
        
def GetMoleculeName(elements):
  for molecule_name in molecules:

    is_match = True
    for element_symbol in elements:

      if not element_symbol in molecules[molecule_name]:
        is_match = False
        break

      if molecules[molecule_name][element_symbol] != elements[element_symbol]:
        is_match = False
        break

    if is_match:
      return molecule_name

  return None


reactions = {}

def CreateReaction(name, reactants, products):
  reaction = {}

  reaction["Catalyst Name"] = name + " Catalyst"

  reaction["Reactants"] = {}
  for reactant_tuple in reactants:
    quantity = reactant_tuple[0]
    elements = reactant_tuple[1]

    molecule_name = GetMoleculeName(elements)
    if molecule_name is None:
      return

    reaction["Reactants"][molecule_name] = quantity
  
  reaction["Products"] = {}
  for product_tuple in products:
    quantity = product_tuple[0]
    elements = product_tuple[1]

    molecule_name = GetMoleculeName(elements)
    if molecule_name is None:
      return

    reaction["Products"][molecule_name] = quantity

  reaction["Ribozyme"] = 0.75
  reaction["Potential"] = 1
  reaction["Flexibility"] = 1
  reaction["Cost"] = 0.0
  reaction["Productivity"] = 1.0
  reaction["Optimal Temperature"] = 298
  reaction["Temperature Tolerance"] = 1.0
  reaction["Thermophilic"] = False
  reaction["Cryophilic"] = False
  reaction["Optimal pH"] = 7.0
  reaction["pH Tolerance"] = 1.0

  reaction["Inhibitors"] = {}
  reaction["Cofactors"] = {}
  

  reactions[name] = reaction

  print(name)

  reaction_string = ""
  for reactant in reaction["Reactants"]:
    reaction_string += str(reaction["Reactants"][reactant]) + " " + reactant + " + "
  reaction_string = reaction_string.rstrip("+ ")
  reaction_string += " => "

  for product in reaction["Products"]:
    reaction_string += str(reaction["Products"][product]) + " " + product + " + "
  reaction_string = reaction_string.rstrip("+ ")

  print(reaction_string + "\n")

def LoadReactions(filename):
  reactants = []
  products = []
  name = None
  
  is_reactant = True

  for line in open(filename, "r").readlines():
    match = re.match("load (.*\\.json) *\n", line)
    if match is not None:
      LoadMolecules(match.group(1))
      continue

    match = re.match("\"(.*)\" *\n", line)
    if match is not None:
      name = match.group(1)
      reactants = []
      products = []

      continue

    if name is not None:
      match = re.match("(.*)=>(.*)", line)
      if match is None:
        continue

      reactant_group_string = match.group(1)
      product_group_string = match.group(2)
      
      for group_string in (reactant_group_string, product_group_string):

        is_reactant = True
        if group_string == product_group_string:
          is_reactant = False
        
        for string in group_string.split("+"):
          quantity = re.match(" *([0-9]+).*", string).group(1)
          elements = {}

          for match in re.finditer("([a-zA-Z]+)([0-9]*)", string):
            element_symbol = match.group(1)
            element_quantity = int(match.group(2)) if not match.group(2) == "" else 1
  
            elements[element_symbol] = element_quantity

          if is_reactant:
            reactants.append((quantity, elements))
          else:
            products.append((quantity, elements))

      CreateReaction(name, reactants, products)
 

filename = sys.argv[1]
seed = sys.argv[2] if len(sys.argv) > 2 else 0

LoadReactions(filename)

json_filepath = "Reactions/" + re.match("(.*)\\.reactions", filename).group(1) + ".json"
json_string = json.dumps({ "Reactions" : reactions }, indent = 2)
#https://stackoverflow.com/questions/46746537/json-force-every-opening-curly-brace-to-appear-in-a-new-separate-line
json_string = re.sub(r"^((\s*)\".*?\":)\s*([\[{])", r"\1\n\2\3", json_string, flags=re.MULTILINE)
json_string = re.sub("},", "},\n", json_string, flags=re.MULTILINE)

open(json_filepath, "w").write(json_string)

