// ****************************************************************************************
//
// Fubi FubiXMLParser
// ---------------------------------------------------------
// Copyright (C) 2010-2013 Felix Kistler 
// 
// This software is distributed under the terms of the Eclipse Public License v1.0.
// A copy of the license may be obtained at: http://www.eclipse.org/org/documents/epl-v10.html
// 
// ****************************************************************************************

#pragma once

// XML parsing
#include "rapidxml.hpp"

// STL containers
#include <string>
#include <vector>

class IGestureRecognizer;
class CombinationRecognizer;

class FubiXMLParser
{
public:
	// Load recognizers out of an xml configuration file
	static bool loadRecognizersFromXML(const std::string& fileName, 
		std::vector<std::pair<std::string, IGestureRecognizer*> >& recognizerContainer,
		std::vector<std::pair<std::string, IGestureRecognizer*> >& hiddenRecognizerContainer,
		std::vector<std::pair<std::string, CombinationRecognizer*> >& combinationRecognizerContainer);

	// Load a combination recognizer from an xml string
	static bool loadCombinationRecognizer(const std::string& xmlDefinition, 
		std::vector<std::pair<std::string, IGestureRecognizer*> >& recognizerContainer,
		std::vector<std::pair<std::string, IGestureRecognizer*> >& hiddenRecognizerContainer,
		std::vector<std::pair<std::string, CombinationRecognizer*> >& combinationRecognizerContainer);

private:
	// Load a combination recognizer from the given xml node
	static bool loadCombinationRecognizerFromXML(rapidxml::xml_node<>* node, float globalMinConfidence, bool globalUseFilteredData,
		std::vector<std::pair<std::string, IGestureRecognizer*> >& recognizerContainer,
		std::vector<std::pair<std::string, IGestureRecognizer*> >& hiddenRecognizerContainer,
		std::vector<std::pair<std::string, CombinationRecognizer*> >& combinationRecognizerContainer);

	// Find a recognizer inside of a container
	static int getRecognizerIndex(const std::string& name, const std::vector< std::pair < std::string, IGestureRecognizer* > >& container);
};