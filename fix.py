#!/usr/bin/env python

#######################################################
# CONFIG SECTION
######################################################

#list of filenames to treat
files_to_treat=[
	"example.xml",
]

#set the folder where the output files will be saved
#it will be created if needed (parents included)
output_folder="output"

#list the tags whose name attribute must be replaced
tags_to_treat=[
	"crosstab",
	"image",
	"text",
	"table",
	"some_tag",
]

#set this to True if you want to treat tags that don't have a name attribute
#i.e. if you want a name attribute to be added if missing
treat_tags_without_name_attribute = False

#initial value of the counter
start_counting_from=1

##########################################################
# END OF CONFIG, CODE STARTS HERE
##########################################################
try:
	import lxml.etree
	import os
	if not os.path.exists(output_folder):
		print "creating output_folder:", output_folder
		os.makedirs(output_folder)

	for fname in files_to_treat:
		print "processing file", fname
		#load the xml file
		xml_doc=lxml.etree.parse(fname)

		#hashtable that contains the counts by tag name
		counts_by_tag={}

		#iterate over the all the nodes (recursively)
		for node in xml_doc.iter():

			if node.tag in tags_to_treat and ( treat_tags_without_name_attribute or "name" in node.attrib ):

				#get the current count in the hash table, default to start_counting_from-1 if not present
				count=counts_by_tag.get(node.tag, start_counting_from-1)

				#increment and record
				count+=1
				counts_by_tag[node.tag]=count

				#set the name attribute
				node.attrib["name"]=node.tag+" "+str(count)

		#save the modified xml document
		output_filename=os.path.join(
			output_folder,
			os.path.basename(fname)
		)
		print "saving file",output_filename
		with open(output_filename,"w") as output:
			output.write(lxml.etree.tostring(xml_doc))
	print "done"
except:
	import traceback
	import sys
	print traceback.format_exc()

raw_input("press enter to finish")
