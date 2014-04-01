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

#include "FubiXMLParser.h"

#include "FubiUtils.h"
#include "FubiRecognizerFactory.h"
#include "GestureRecognizer/CombinationRecognizer.h"

// File reading for Xml parsing
#include <fstream>


using namespace Fubi;


bool FubiXMLParser::loadCombinationRecognizer(const std::string& xmlDefinition, 
	std::vector<std::pair<std::string, IGestureRecognizer*> >& recognizerContainer,
	std::vector<std::pair<std::string, IGestureRecognizer*> >& hiddenRecognizerContainer,
	std::vector<std::pair<std::string, CombinationRecognizer*> >& combinationRecognizerContainer)
{
	bool loaded = false;

	// copy string to buffer
	char* buffer = new char [xmlDefinition.length()+1];
#pragma warning(push)
#pragma warning(disable:4996)
	strcpy(buffer, xmlDefinition.c_str());
#pragma warning(pop)

	// parse XML
	rapidxml::xml_document<> doc;
	doc.parse<0>(buffer);
	rapidxml::xml_node<>* node = doc.first_node("CombinationRecognizer");
	if (node)
	{
		loaded = loadCombinationRecognizerFromXML(node, -1, false, recognizerContainer, hiddenRecognizerContainer, combinationRecognizerContainer);
	}

	// release xml doc and buffer
	doc.clear();
	delete[] buffer;

	return loaded;
}

bool FubiXMLParser::loadRecognizersFromXML(const std::string& fileName, 
		std::vector<std::pair<std::string, IGestureRecognizer*> >& recognizerContainer,
		std::vector<std::pair<std::string, IGestureRecognizer*> >& hiddenRecognizerContainer,
		std::vector<std::pair<std::string, CombinationRecognizer*> >& combinationRecognizerContainer)
{
	// Open the file and copy the data to a buffer
	fstream file;
	file.open (fileName.c_str(), fstream::in | fstream::binary );

	if (!file.is_open() || !file.good())
		return false;

	bool loadedAnything = false;

	// get length of file:
	file.seekg (0, fstream::end);
	int length = (int)file.tellg();
	file.seekg (0, fstream::beg);
	// allocate memory:
	char* buffer = new char [length+1];
	// read data as a block:
	file.read(buffer, length);
	// null terminate the string
	buffer[length] = '\0';
	// and close the file
	file.close();

	// Load the string to the parser
	rapidxml::xml_document<> doc;    // character type defaults to char
	doc.parse<0>(buffer);

	// Parse the content
	rapidxml::xml_node<>* node = doc.first_node("FubiRecognizers");
	if (node)
	{
		float globalMinConf = -1.0f;
		rapidxml::xml_attribute<>* globalMinConfA = node->first_attribute("globalMinConfidence");
		if (globalMinConfA)
			globalMinConf = (float)atof(globalMinConfA->value());

		bool globalUseFilteredData = false;
		rapidxml::xml_attribute<>* globalfilterA = node->first_attribute("globalUseFilteredData");
		if (globalfilterA)
			globalUseFilteredData = strcmp(globalfilterA->value(), "false") != 0 && strcmp(globalfilterA->value(), "0") != 0;

		rapidxml::xml_node<>* recNode;
		for(recNode = node->first_node("JointRelationRecognizer"); recNode; recNode = recNode->next_sibling("JointRelationRecognizer"))
		{
			std::string name;
			rapidxml::xml_attribute<>* attr = recNode->first_attribute("name");
			if (attr)
				name = attr->value();

			bool visible = true;
			attr = recNode->first_attribute("visibility");
			if (attr)
				visible = removeWhiteSpacesAndToLower(attr->value()) != "hidden";

			bool localPos = false;
			attr = recNode->first_attribute("useLocalPositions");
			if (attr)
				localPos = removeWhiteSpacesAndToLower(attr->value()) != "false";

			float minConf = globalMinConf;
			rapidxml::xml_attribute<>* minConfA = recNode->first_attribute("minConfidence");
			if (minConfA)
				minConf = (float)atof(minConfA->value());

			bool useFilteredData = globalUseFilteredData;
			rapidxml::xml_attribute<>* filterA = recNode->first_attribute("useFilteredData");
			if (filterA)
				useFilteredData = strcmp(filterA->value(), "false") != 0 && strcmp(filterA->value(), "0") != 0;

			BodyMeasurement::Measurement measure = BodyMeasurement::NUM_MEASUREMENTS;
			rapidxml::xml_attribute<>* measuringUnit = recNode->first_attribute("measuringUnit");
			if (measuringUnit)
				measure = Fubi::getBodyMeasureID(measuringUnit->value());

			SkeletonJoint::Joint joint = SkeletonJoint::RIGHT_HAND;
			SkeletonJoint::Joint relJoint = SkeletonJoint::NUM_JOINTS;
			rapidxml::xml_node<>* jointNode = recNode->first_node("Joints");
			if (jointNode)
			{
				attr = jointNode->first_attribute("main");
				if (attr)
					joint = getJointID(attr->value());
				attr = jointNode->first_attribute("relative");
				if (attr)
					relJoint = getJointID(attr->value());
			}
			RecognizerTarget::Target targetSensor = RecognizerTarget::BODY_SENSOR;
			rapidxml::xml_node<>* handJointNode = recNode->first_node("HandJoints");
			if (handJointNode)
			{
				attr = handJointNode->first_attribute("main");
				if (attr)
				{
					SkeletonHandJoint::Joint hJoint = getHandJointID(attr->value());
					// This is the hacky part, converting a hand joint enum to a skeleton joint enum
					// We only care about the actual digit...
					if (hJoint != SkeletonHandJoint::NUM_JOINTS)
					{
						joint = (SkeletonJoint::Joint) hJoint;
						targetSensor = RecognizerTarget::FINGER_SENSOR;
						attr = handJointNode->first_attribute("relative");
						if (attr)
						{
							hJoint = getHandJointID(attr->value());
							if (hJoint == SkeletonHandJoint::NUM_JOINTS)
								relJoint = SkeletonJoint::NUM_JOINTS;
							else
								relJoint = (SkeletonJoint::Joint) hJoint;
						}
					}
				}
			}

			Vec3f minValues = DefaultMinVec;
			float minDistance = 0;
			rapidxml::xml_node<>* minNode = recNode->first_node("MinValues");
			if (minNode)
			{
				attr = minNode->first_attribute("x");
				if (attr)
					minValues.x = (float) atof(attr->value());
				attr = minNode->first_attribute("y");
				if (attr)
					minValues.y = (float) atof(attr->value());
				attr = minNode->first_attribute("z");
				if (attr)
					minValues.z = (float) atof(attr->value());

				attr = minNode->first_attribute("dist");
				if (attr)
					minDistance = (float) atof(attr->value());
			}

			Vec3f maxValues = DefaultMaxVec;
			float maxDistance = Math::MaxFloat;
			rapidxml::xml_node<>* maxNode = recNode->first_node("MaxValues");
			if (maxNode)
			{
				attr = maxNode->first_attribute("x");
				if (attr)
					maxValues.x = (float) atof(attr->value());
				attr = maxNode->first_attribute("y");
				if (attr)
					maxValues.y = (float) atof(attr->value());
				attr = maxNode->first_attribute("z");
				if (attr)
					maxValues.z = (float) atof(attr->value());

				attr = maxNode->first_attribute("dist");
				if (attr)
					maxDistance = (float) atof(attr->value());
			}


			for(rapidxml::xml_node<>* relNode = recNode->first_node("Relation"); relNode; relNode = relNode->next_sibling("Relation"))
			{
				float min = -Math::MaxFloat;
				float max = Math::MaxFloat;
				attr = relNode->first_attribute("min");
				if (attr)
					min = (float) atof(attr->value());
				attr = relNode->first_attribute("max");
				if (attr)
					max = (float) atof(attr->value());
				attr = relNode->first_attribute("type");
				if (attr)
				{
					std::string lowerValue = removeWhiteSpacesAndToLower(attr->value());
                    if (lowerValue == "infrontof")
					{
						maxValues.z = minf(maxValues.z, -min);
						minValues.z = maxf(minValues.z, -max);
					}
                    else if (lowerValue == "behind")
					{
						maxValues.z = minf(maxValues.z, max);
						minValues.z = maxf(minValues.z, min);
					}
                    else if (lowerValue == "leftof")
					{
						maxValues.x = minf(maxValues.x, -min);
						minValues.x = maxf(minValues.x, -max);
					}
                    else if (lowerValue == "rightof")
					{
						maxValues.x = minf(maxValues.x, max);
						minValues.x = maxf(minValues.x, min);
					}
                    else if (lowerValue == "above")
					{
						maxValues.y = minf(maxValues.y, max);
						minValues.y = maxf(minValues.y, min);
					}
                    else if (lowerValue == "below")
					{
						maxValues.y = minf(maxValues.y, -min);
						minValues.y = maxf(minValues.y, -max);
					}
                    else if (lowerValue == "apartof")
					{
						minDistance = min;
						maxDistance = max;
					}
				}
			}

			IGestureRecognizer* rec = createPostureRecognizer(joint, relJoint, minValues, maxValues, minDistance, maxDistance, localPos, minConf, measure, useFilteredData);
			rec->m_targetSensor = targetSensor;
			if (visible)
				recognizerContainer.push_back(pair<string, IGestureRecognizer*>(name, rec));
			else
				hiddenRecognizerContainer.push_back(pair<string, IGestureRecognizer*>(name, rec));
			loadedAnything = true;
		}

		for(recNode = node->first_node("JointOrientationRecognizer"); recNode; recNode = recNode->next_sibling("JointOrientationRecognizer"))
		{
			std::string name;
			rapidxml::xml_attribute<>* attr = recNode->first_attribute("name");
			if (attr)
				name = attr->value();

			bool visible = true;
			attr = recNode->first_attribute("visibility");
			if (attr)
				visible = removeWhiteSpacesAndToLower(attr->value()) != "hidden";

			bool localRot = true;
			attr = recNode->first_attribute("useLocalOrientations");
			if (attr)
				localRot = removeWhiteSpacesAndToLower(attr->value()) != "false";

			float minConf = globalMinConf;
			rapidxml::xml_attribute<>* minConfA = recNode->first_attribute("minConfidence");
			if (minConfA)
				minConf = (float)atof(minConfA->value());

			bool useFilteredData = globalUseFilteredData;
			rapidxml::xml_attribute<>* filterA = recNode->first_attribute("useFilteredData");
			if (filterA)
				useFilteredData = strcmp(filterA->value(), "false") != 0 && strcmp(filterA->value(), "0") != 0;

			SkeletonJoint::Joint joint = SkeletonJoint::TORSO;
			rapidxml::xml_node<>* jointNode = recNode->first_node("Joint");
			if (jointNode)
			{
				attr = jointNode->first_attribute("name");
				if (attr)
					joint = getJointID(attr->value());
			}
			RecognizerTarget::Target targetSensor = RecognizerTarget::BODY_SENSOR;
			rapidxml::xml_node<>* handJointNode = recNode->first_node("HandJoint");
			if (handJointNode)
			{
				attr = handJointNode->first_attribute("name");
				if (attr)
				{
					SkeletonHandJoint::Joint hJoint = getHandJointID(attr->value());
					// This is the hacky part, converting a hand joint enum to a skeleton joint enum
					// We only care about the actual digit...
					if (hJoint != SkeletonHandJoint::NUM_JOINTS)
					{
						joint = (SkeletonJoint::Joint) hJoint;
						targetSensor = RecognizerTarget::FINGER_SENSOR;
					}
				}
			}

			bool useOrientation = false;
			Vec3f orient(Math::NO_INIT);
			float maxAngleDiff = 45.0f;
			rapidxml::xml_node<>* orientNode = recNode->first_node("Orientation");
			if (orientNode)
			{
				useOrientation = true;
				attr = orientNode->first_attribute("x");
				if (attr)
					orient.x = (float) atof(attr->value());
				attr = orientNode->first_attribute("y");
				if (attr)
					orient.y = (float) atof(attr->value());
				attr = orientNode->first_attribute("z");
				if (attr)
					orient.z = (float) atof(attr->value());
				attr = orientNode->first_attribute("maxAngleDifference");
				if (attr)
					maxAngleDiff = (float) atof(attr->value());
			}

			if (useOrientation)
			{
				if (visible)
					recognizerContainer.push_back(pair<string, IGestureRecognizer*>(name, createPostureRecognizer(joint, orient, maxAngleDiff, localRot, minConf, useFilteredData)));
				else
					hiddenRecognizerContainer.push_back(pair<string, IGestureRecognizer*>(name, createPostureRecognizer(joint, orient, maxAngleDiff, localRot, minConf, useFilteredData)));
				loadedAnything = true;
			}
			else
			{
				Vec3f minValues = Vec3f(-180.0f, -180.0f, -180.0f);
				rapidxml::xml_node<>* minNode = recNode->first_node("MinDegrees");
				if (minNode)
				{
					attr = minNode->first_attribute("x");
					if (attr)
						minValues.x = (float) atof(attr->value());
					attr = minNode->first_attribute("y");
					if (attr)
						minValues.y = (float) atof(attr->value());
					attr = minNode->first_attribute("z");
					if (attr)
						minValues.z = (float) atof(attr->value());
				}

				Vec3f maxValues = Vec3f(180.0f, 180.0f, 180.0f);
				rapidxml::xml_node<>* maxNode = recNode->first_node("MaxDegrees");
				if (maxNode)
				{
					attr = maxNode->first_attribute("x");
					if (attr)
						maxValues.x = (float) atof(attr->value());
					attr = maxNode->first_attribute("y");
					if (attr)
						maxValues.y = (float) atof(attr->value());
					attr = maxNode->first_attribute("z");
					if (attr)
						maxValues.z = (float) atof(attr->value());
				}
			
				IGestureRecognizer* rec = createPostureRecognizer(joint, minValues, maxValues, localRot, minConf, useFilteredData);
				rec->m_targetSensor = targetSensor;
				if (visible)
					recognizerContainer.push_back(pair<string, IGestureRecognizer*>(name, rec));
				else
					hiddenRecognizerContainer.push_back(pair<string, IGestureRecognizer*>(name, rec));
				loadedAnything = true;
			}
		}
		
		for(recNode = node->first_node("LinearMovementRecognizer"); recNode; recNode = recNode->next_sibling("LinearMovementRecognizer"))
		{
			std::string name;
			rapidxml::xml_attribute<>* attr = recNode->first_attribute("name");
			if (attr)
				name = attr->value();

			bool visible = true;
			attr = recNode->first_attribute("visibility");
			if (attr)
				visible = removeWhiteSpacesAndToLower(attr->value()) != "hidden";

			bool localPos = false;
			attr = recNode->first_attribute("useLocalPositions");
			if (attr)
				localPos = removeWhiteSpacesAndToLower(attr->value()) != "false";

			float minConf = globalMinConf;
			rapidxml::xml_attribute<>* minConfA = recNode->first_attribute("minConfidence");
			if (minConfA)
				minConf = (float)atof(minConfA->value());

			bool useFilteredData = globalUseFilteredData;
			rapidxml::xml_attribute<>* filterA = recNode->first_attribute("useFilteredData");
			if (filterA)
				useFilteredData = strcmp(filterA->value(), "false") != 0 && strcmp(filterA->value(), "0") != 0;

			bool useOnlyCorrectDirectionComponent = true;
			attr = recNode->first_attribute("useOnlyCorrectDirectionComponent");
			if (attr)
				useOnlyCorrectDirectionComponent = removeWhiteSpacesAndToLower(attr->value()) != "false";

			SkeletonJoint::Joint joint = SkeletonJoint::RIGHT_HAND;
			SkeletonJoint::Joint relJoint = SkeletonJoint::NUM_JOINTS;
			bool useRelative = false;
			rapidxml::xml_node<>* jointNode = recNode->first_node("Joints");
			if (jointNode)
			{
				attr = jointNode->first_attribute("main");
				if (attr)
					joint = getJointID(attr->value());
				attr = jointNode->first_attribute("relative");
				if (attr)
				{
					relJoint = getJointID(attr->value());
					useRelative = true;
				}
			}
			RecognizerTarget::Target targetSensor = RecognizerTarget::BODY_SENSOR;
			rapidxml::xml_node<>* handJointNode = recNode->first_node("HandJoints");
			if (handJointNode)
			{
				attr = handJointNode->first_attribute("main");
				if (attr)
				{
					SkeletonHandJoint::Joint hJoint = getHandJointID(attr->value());
					// This is the hacky part, converting a hand joint enum to a skeleton joint enum
					// We only care about the actual digit...
					if (hJoint != SkeletonHandJoint::NUM_JOINTS)
					{
						joint = (SkeletonJoint::Joint) hJoint;
						targetSensor = RecognizerTarget::FINGER_SENSOR;
						attr = handJointNode->first_attribute("relative");
						if (attr)
						{
							hJoint = getHandJointID(attr->value());
							if (hJoint == SkeletonHandJoint::NUM_JOINTS)
								relJoint = SkeletonJoint::NUM_JOINTS;
							else
								relJoint = (SkeletonJoint::Joint) hJoint;
						}
					}
				}
			}

			Vec3f direction;
			float minVel = 0;
			float maxVel = Math::MaxFloat;
			float maxAngleDiff = 45.0f;
			float minLength = 0;
			float maxLength = Math::MaxFloat;

			rapidxml::xml_node<>* dirNode = recNode->first_node("Direction");
			if (dirNode)
			{
				attr = dirNode->first_attribute("x");
				if (attr)
					direction.x = (float) atof(attr->value());
				attr = dirNode->first_attribute("y");
				if (attr)
					direction.y = (float) atof(attr->value());
				attr = dirNode->first_attribute("z");
				if (attr)
					direction.z = (float) atof(attr->value());
				attr = dirNode->first_attribute("maxAngleDifference");
				if (attr)
					maxAngleDiff = (float) atof(attr->value());
			}

			dirNode = recNode->first_node("BasicDirection");
			if (dirNode)
			{
				attr = dirNode->first_attribute("type");
				if (attr)
				{
					std::string lowerValue = removeWhiteSpacesAndToLower(attr->value());
                    if (lowerValue == "left")
					{
						direction = Vec3f(-1.0f, 0, 0);
					}
                    else if (lowerValue == "right")
					{
						direction = Vec3f(1.0f, 0, 0);
					}
                    else if (lowerValue == "up")
					{
						direction = Vec3f(0, 1.0f, 0);
					}
                    else if (lowerValue == "down")
					{
						direction = Vec3f(0, -1.0f, 0);
					}
                    else if (lowerValue == "forward")
					{
						direction = Vec3f(0, 0, -1.0f);
					}
                    else if (lowerValue == "backward")
					{
						direction = Vec3f(0, 0, 1.0f);
					}
                    else if (lowerValue == "anydirection")
					{
						direction = Vec3f(0, 0, 0);
					}
				}
				attr = dirNode->first_attribute("maxAngleDifference");
				if (attr)
					maxAngleDiff = (float) atof(attr->value());
			}

			rapidxml::xml_node<>* speedNode = recNode->first_node("Speed");
			if (speedNode)
			{
				attr = speedNode->first_attribute("min");
				if (attr)
					minVel = (float) atof(attr->value());
				attr = speedNode->first_attribute("max");
				if (attr)
					maxVel = (float) atof(attr->value());
			}

			bool useLength = false;
			BodyMeasurement::Measurement measure = BodyMeasurement::NUM_MEASUREMENTS;
			rapidxml::xml_node<>* lengthNode = recNode->first_node("Length");
			if (lengthNode)
			{
				useLength = true;
				attr = lengthNode->first_attribute("min");
				if (attr)
					minLength = (float) atof(attr->value());
				attr = lengthNode->first_attribute("max");
				if (attr)
					maxLength = (float) atof(attr->value());
				attr = lengthNode->first_attribute("measuringUnit");
				if (attr)
					measure = Fubi::getBodyMeasureID(attr->value());
			}

			IGestureRecognizer* rec = 0x0;
			if (useRelative)
			{
				if (useLength)
					rec = createMovementRecognizer(joint, relJoint, direction, minVel, maxVel, minLength, maxLength, measure, localPos, minConf, maxAngleDiff, useOnlyCorrectDirectionComponent, useFilteredData);
				else
					rec = createMovementRecognizer(joint, relJoint, direction, minVel, maxVel, localPos, minConf, maxAngleDiff, useOnlyCorrectDirectionComponent, useFilteredData);
			}
			else
			{
				if (useLength)
					rec = createMovementRecognizer(joint, direction, minVel, maxVel, minLength, maxLength, measure, localPos, minConf, maxAngleDiff, useOnlyCorrectDirectionComponent, useFilteredData);
				else
					rec = createMovementRecognizer(joint, direction, minVel, maxVel, localPos, minConf, maxAngleDiff, useOnlyCorrectDirectionComponent, useFilteredData);
			}
			if (rec)
			{
				rec->m_targetSensor = targetSensor;
				if (visible)
					recognizerContainer.push_back(pair<string, IGestureRecognizer*>(name, rec));
				else
					hiddenRecognizerContainer.push_back(pair<string, IGestureRecognizer*>(name, rec));
				loadedAnything = true;
			}
		}

		for(recNode = node->first_node("AngularMovementRecognizer"); recNode; recNode = recNode->next_sibling("AngularMovementRecognizer"))
		{
			std::string name;
			rapidxml::xml_attribute<>* attr = recNode->first_attribute("name");
			if (attr)
				name = attr->value();

			bool visible = true;
			attr = recNode->first_attribute("visibility");
			if (attr)
				visible = removeWhiteSpacesAndToLower(attr->value()) != "hidden";

			bool localRot = true;
			attr = recNode->first_attribute("useLocalOrientations");
			if (attr)
				localRot = removeWhiteSpacesAndToLower(attr->value()) != "false";

			float minConf = globalMinConf;
			rapidxml::xml_attribute<>* minConfA = recNode->first_attribute("minConfidence");
			if (minConfA)
				minConf = (float)atof(minConfA->value());

			bool useFilteredData = globalUseFilteredData;
			rapidxml::xml_attribute<>* filterA = recNode->first_attribute("useFilteredData");
			if (filterA)
				useFilteredData = strcmp(filterA->value(), "false") != 0 && strcmp(filterA->value(), "0") != 0;

			SkeletonJoint::Joint joint = SkeletonJoint::TORSO;
			rapidxml::xml_node<>* jointNode = recNode->first_node("Joint");
			if (jointNode)
			{
				attr = jointNode->first_attribute("name");
				if (attr)
					joint = getJointID(attr->value());
			}
			RecognizerTarget::Target targetSensor = RecognizerTarget::BODY_SENSOR;
			rapidxml::xml_node<>* handJointNode = recNode->first_node("HandJoint");
			if (handJointNode)
			{
				attr = handJointNode->first_attribute("name");
				if (attr)
				{
					SkeletonHandJoint::Joint hJoint = getHandJointID(attr->value());
					// This is the hacky part, converting a hand joint enum to a skeleton joint enum
					// We only care about the actual digit...
					if (hJoint != SkeletonHandJoint::NUM_JOINTS)
					{
						joint = (SkeletonJoint::Joint) hJoint;
						targetSensor = RecognizerTarget::FINGER_SENSOR;
					}
				}
			}

			Vec3f minVel = Fubi::DefaultMinVec;
			rapidxml::xml_node<>* minNode = recNode->first_node("MinAngularVelocity");
			if (minNode)
			{
				attr = minNode->first_attribute("x");
				if (attr)
					minVel.x = (float) atof(attr->value());
				attr = minNode->first_attribute("y");
				if (attr)
					minVel.y = (float) atof(attr->value());
				attr = minNode->first_attribute("z");
				if (attr)
					minVel.z = (float) atof(attr->value());
			}

			Vec3f maxVel = Fubi::DefaultMaxVec;
			rapidxml::xml_node<>* maxNode = recNode->first_node("MaxAngularVelocity");
			if (maxNode)
			{
				attr = maxNode->first_attribute("x");
				if (attr)
					maxVel.x = (float) atof(attr->value());
				attr = maxNode->first_attribute("y");
				if (attr)
					maxVel.y = (float) atof(attr->value());
				attr = maxNode->first_attribute("z");
				if (attr)
					maxVel.z = (float) atof(attr->value());
			}

			for(rapidxml::xml_node<>* basicNode = recNode->first_node("BasicAngularVelocity"); basicNode; basicNode = basicNode->next_sibling("BasicAngularVelocity"))
			{
				float min = -Math::MaxFloat;
				float max = Math::MaxFloat;
				attr = basicNode->first_attribute("min");
				if (attr)
					min = (float) atof(attr->value());
				attr = basicNode->first_attribute("max");
				if (attr)
					max = (float) atof(attr->value());
				attr = basicNode->first_attribute("type");
				if (attr)
				{
					std::string lowerValue = removeWhiteSpacesAndToLower(attr->value());
                    if (lowerValue == "rollleft")
					{
						maxVel.z = minf(maxVel.z, max);
						minVel.z = maxf(minVel.z, min);
					}
                    else if (lowerValue == "rollright")
					{
						maxVel.z = minf(maxVel.z, -min);
						minVel.z = maxf(minVel.z, -max);
					}
                    else if (lowerValue == "pitchdown")
					{
						maxVel.x = minf(maxVel.x, -min);
						minVel.x = maxf(minVel.x, -max);
					}
                    else if (lowerValue == "pitchup")
					{
						maxVel.x = minf(maxVel.x, max);
						minVel.x = maxf(minVel.x, min);
					}
                    else if (lowerValue == "yawright")
					{
						maxVel.y = minf(maxVel.y, max);
						minVel.y = maxf(minVel.y, min);
					}
                    else if (lowerValue == "yawleft")
					{
						maxVel.y = minf(maxVel.y, -min);
						minVel.y = maxf(minVel.y, -max);
					}
				}
			}			

			IGestureRecognizer* rec = createMovementRecognizer(joint, minVel, maxVel, localRot, minConf, useFilteredData);
			rec->m_targetSensor = targetSensor;
			if (visible)
				recognizerContainer.push_back(pair<string, IGestureRecognizer*>(name, rec));
			else
				hiddenRecognizerContainer.push_back(pair<string, IGestureRecognizer*>(name, rec));
			loadedAnything = true;
		}

		for(recNode = node->first_node("LinearAccelerationRecognizer"); recNode; recNode = recNode->next_sibling("LinearAccelerationRecognizer"))
		{
			std::string name;
			rapidxml::xml_attribute<>* attr = recNode->first_attribute("name");
			if (attr)
				name = attr->value();

			bool visible = true;
			attr = recNode->first_attribute("visibility");
			if (attr)
				visible = removeWhiteSpacesAndToLower(attr->value()) != "hidden";


			float minConf = globalMinConf;
			rapidxml::xml_attribute<>* minConfA = recNode->first_attribute("minConfidence");
			if (minConfA)
				minConf = (float)atof(minConfA->value());

			bool useOnlyCorrectDirectionComponent = true;
			attr = recNode->first_attribute("useOnlyCorrectDirectionComponent");
			if (attr)
				useOnlyCorrectDirectionComponent = removeWhiteSpacesAndToLower(attr->value()) != "false";

			SkeletonJoint::Joint joint = SkeletonJoint::RIGHT_HAND;
			rapidxml::xml_node<>* jointNode = recNode->first_node("Joint");
			if (jointNode)
			{
				attr = jointNode->first_attribute("name");
				if (attr)
					joint = getJointID(attr->value());
			}

			Vec3f direction;
			float minAccel = 0;
			float maxAccel = Math::MaxFloat;
			float maxAngleDiff = 45.0f;

			rapidxml::xml_node<>* dirNode = recNode->first_node("Direction");
			if (dirNode)
			{
				attr = dirNode->first_attribute("x");
				if (attr)
					direction.x = (float) atof(attr->value());
				attr = dirNode->first_attribute("y");
				if (attr)
					direction.y = (float) atof(attr->value());
				attr = dirNode->first_attribute("z");
				if (attr)
					direction.z = (float) atof(attr->value());
				attr = dirNode->first_attribute("maxAngleDifference");
				if (attr)
					maxAngleDiff = (float) atof(attr->value());
			}

			dirNode = recNode->first_node("BasicDirection");
			if (dirNode)
			{
				attr = dirNode->first_attribute("type");
				if (attr)
				{
					std::string lowerValue = removeWhiteSpacesAndToLower(attr->value());
                    if (lowerValue == "left")
					{
						direction = Vec3f(-1.0f, 0, 0);
					}
                    else if (lowerValue == "right")
					{
						direction = Vec3f(1.0f, 0, 0);
					}
                    else if (lowerValue == "up")
					{
						direction = Vec3f(0, 1.0f, 0);
					}
                    else if (lowerValue == "down")
					{
						direction = Vec3f(0, -1.0f, 0);
					}
                    else if (lowerValue == "forward")
					{
						direction = Vec3f(0, 0, -1.0f);
					}
                    else if (lowerValue == "backward")
					{
						direction = Vec3f(0, 0, 1.0f);
					}
                    else if (lowerValue == "anydirection")
					{
						direction = Vec3f(0, 0, 0);
					}
				}
				attr = dirNode->first_attribute("maxAngleDifference");
				if (attr)
					maxAngleDiff = (float) atof(attr->value());
			}

			rapidxml::xml_node<>* speedNode = recNode->first_node("Acceleration");
			if (speedNode)
			{
				attr = speedNode->first_attribute("min");
				if (attr)
					minAccel = (float) atof(attr->value());
				attr = speedNode->first_attribute("max");
				if (attr)
					maxAccel = (float) atof(attr->value());
			}

			if (visible)
				recognizerContainer.push_back(pair<string, IGestureRecognizer*>(name, createMovementRecognizer(joint, direction, minAccel, maxAccel, minConf, maxAngleDiff, useOnlyCorrectDirectionComponent)));
			else
				hiddenRecognizerContainer.push_back(pair<string, IGestureRecognizer*>(name, createMovementRecognizer(joint, direction, minAccel, maxAccel, minConf, maxAngleDiff, useOnlyCorrectDirectionComponent)));
			loadedAnything = true;
		}

		for(recNode = node->first_node("FingerCountRecognizer"); recNode; recNode = recNode->next_sibling("FingerCountRecognizer"))
		{
			std::string name;
			rapidxml::xml_attribute<>* attr = recNode->first_attribute("name");
			if (attr)
				name = attr->value();

			bool visible = true;
			attr = recNode->first_attribute("visibility");
			if (attr)
				visible = removeWhiteSpacesAndToLower(attr->value()) != "hidden";

			float minConf = globalMinConf;
			rapidxml::xml_attribute<>* minConfA = recNode->first_attribute("minConfidence");
			if (minConfA)
				minConf = (float)atof(minConfA->value());

			bool useFilteredData = globalUseFilteredData;
			rapidxml::xml_attribute<>* filterA = recNode->first_attribute("useFilteredData");
			if (filterA)
				useFilteredData = strcmp(filterA->value(), "false") != 0 && strcmp(filterA->value(), "0") != 0;

			
			SkeletonJoint::Joint joint = SkeletonJoint::NUM_JOINTS;
			rapidxml::xml_node<>* jointNode = recNode->first_node("Joint");
			if (jointNode)
			{
				attr = jointNode->first_attribute("name");
				if (attr)
					joint = getJointID(attr->value());
			}

			unsigned int minFingers = 0;
			unsigned int maxFingers = 5;
			bool useMedian = false;
			rapidxml::xml_node<>* countNode = recNode->first_node("FingerCount");
			if (countNode)
			{
				attr = countNode->first_attribute("min");
				if (attr)
					minFingers = (unsigned)atoi(attr->value());
				attr = countNode->first_attribute("max");
				if (attr)
					maxFingers = (unsigned)atoi(attr->value());
				attr = countNode->first_attribute("useMedianCalculation");
				if (attr)
				{
					std::string lowerValue = removeWhiteSpacesAndToLower(attr->value());
					useMedian = lowerValue != "0" && lowerValue != "false";
				}
			}

			IGestureRecognizer* rec = createPostureRecognizer(joint, minFingers, maxFingers, minConf, useMedian, useFilteredData);
			rec->m_targetSensor = RecognizerTarget::ALL_SENSORS;
			if (visible)
				recognizerContainer.push_back(pair<string, IGestureRecognizer*>(name, rec));
			else
				hiddenRecognizerContainer.push_back(pair<string, IGestureRecognizer*>(name, rec));
			loadedAnything = true;
		}

		for(recNode = node->first_node("CombinationRecognizer"); recNode; recNode = recNode->next_sibling("CombinationRecognizer"))
		{
			if (loadCombinationRecognizerFromXML(recNode, globalMinConf, globalUseFilteredData, recognizerContainer, hiddenRecognizerContainer, combinationRecognizerContainer))
				loadedAnything = true;
		}

		bool oldCombinations = false;
		for(recNode = node->first_node("PostureCombinationRecognizer"); recNode; recNode = recNode->next_sibling("PostureCombinationRecognizer"))
		{
			if (loadCombinationRecognizerFromXML(recNode, globalMinConf, globalUseFilteredData, recognizerContainer, hiddenRecognizerContainer, combinationRecognizerContainer))
			{
				loadedAnything = true;
				oldCombinations = true;
			}
		}
		if (oldCombinations)
		{
			Fubi_logWrn("XML_Warning - \"PostureCombinationRecognizer\" deprecated, please use \"CombinationRecognizer\"!\n");
		}
	}

	doc.clear();
	// release the buffer
	delete[] buffer;
	return loadedAnything;
}

bool FubiXMLParser::loadCombinationRecognizerFromXML(rapidxml::xml_node<>* node, float globalMinConfidence, bool globalUseFilteredData, 
	std::vector<std::pair<std::string, IGestureRecognizer*> >& recognizerContainer,
	std::vector<std::pair<std::string, IGestureRecognizer*> >& hiddenRecognizerContainer,
	std::vector<std::pair<std::string, CombinationRecognizer*> >& combinationRecognizerContainer)
{
	bool success = false;

	std::string name;
	rapidxml::xml_attribute<>* attr = node->first_attribute("name");
	if (attr)
	{
		name = attr->value();

		// Create combination recognizer template (not assigned to a user or hand)
		CombinationRecognizer* rec = new CombinationRecognizer(name);

		attr = node->first_attribute("waitUntilLastStateRecognizersStop");
		if (attr)
		{
			std::string lowerValue = removeWhiteSpacesAndToLower(attr->value());
			rec->setWaitUntilLastStateRecognizersStop(lowerValue != "0" && lowerValue != "false");
		}

		rapidxml::xml_node<>* stateNode;
		int stateNum;
		for(stateNode = node->first_node("State"), stateNum = 1; stateNode; stateNode = stateNode->next_sibling("State"), stateNum++)
		{
			double maxDuration = -1;
			double minDuration = 0;
			double timeForTransition = 1.0;
			double maxInterruption = -1;
			bool noInterrruptionBeforeMinDuration = false;
			bool restartOnFail = true;

			attr = stateNode->first_attribute("maxDuration");
			if (attr)
				maxDuration = atof(attr->value());
			attr = stateNode->first_attribute("minDuration");
			if (attr)
				minDuration = atof(attr->value());
			attr = stateNode->first_attribute("timeForTransition");
			if (attr)
				timeForTransition = atof(attr->value());
			attr = stateNode->first_attribute("maxInterruptionTime");
			if (attr)
				maxInterruption = atof(attr->value());
			attr = stateNode->first_attribute("noInterrruptionBeforeMinDuration");
			if (attr)
			{
				std::string lowerValue = removeWhiteSpacesAndToLower(attr->value());
				noInterrruptionBeforeMinDuration = lowerValue != "0" && lowerValue != "false";
			}
			attr = stateNode->first_attribute("onFail");
			if (attr)
				restartOnFail = removeWhiteSpacesAndToLower(attr->value()) != "goback";

			std::vector<IGestureRecognizer*> recognizerRefs;
			rapidxml::xml_node<>* recRefNode;
			for(recRefNode = stateNode->first_node("Recognizer"); recRefNode; recRefNode = recRefNode->next_sibling("Recognizer"))
			{
				std::string name;
				rapidxml::xml_attribute<>* attr = recRefNode->first_attribute("name");
				if (attr)
				{
					bool ignoreOnTrackingError = false;
					rapidxml::xml_attribute<>* attr1 = recRefNode->first_attribute("ignoreOnTrackingError");
					if (attr1)
					{
						std::string lowerValue = removeWhiteSpacesAndToLower(attr1->value());
						ignoreOnTrackingError = lowerValue != "0" && lowerValue != "false";
					}

					float minConf = -1.0f;
					rapidxml::xml_attribute<>* minConfA = recRefNode->first_attribute("minConfidence");
					if (minConfA)
						minConf = (float)atof(minConfA->value());

					bool useFilteredData = false;
					rapidxml::xml_attribute<>* filterA = recRefNode->first_attribute("useFilteredData");
					if (filterA)
						useFilteredData = strcmp(filterA->value(), "false") != 0 && strcmp(filterA->value(), "0") != 0;

					int index = getRecognizerIndex(attr->value(), recognizerContainer);
					if (index > -1)
					{
						// found it
						recognizerRefs.push_back(recognizerContainer[index].second->clone());
						recognizerRefs.back()->m_ignoreOnTrackingError = ignoreOnTrackingError;
						if (minConf >= 0)
							recognizerRefs.back()->m_minConfidence = minConf;
						if (filterA)
							recognizerRefs.back()->m_useFilteredData = useFilteredData;
					}
					else
					{
						index = getRecognizerIndex(attr->value(), hiddenRecognizerContainer);
						if (index > -1)
						{
							// found it
							recognizerRefs.push_back(hiddenRecognizerContainer[index].second->clone());
							recognizerRefs.back()->m_ignoreOnTrackingError = ignoreOnTrackingError;
							if (minConf >= 0)
								recognizerRefs.back()->m_minConfidence = minConf;
							if (filterA)
								recognizerRefs.back()->m_useFilteredData = useFilteredData;
						}
						else
						{
							index = atoi(attr->value());
							if ((index > 0 || (index == 0 && attr->value()[0] == '0')) && (unsigned) index < recognizerContainer.size())
							{
								// name in fact represents the index of a recognizer
								recognizerRefs.push_back(recognizerContainer[index].second->clone());
								recognizerRefs.back()->m_ignoreOnTrackingError = ignoreOnTrackingError;
								if (minConf >= 0)
									recognizerRefs.back()->m_minConfidence = minConf;
								if (filterA)
									recognizerRefs.back()->m_useFilteredData = useFilteredData;
							}
							else // last option: name belongs to a predefined gesture
							{
								Postures::Posture p = getPostureID(attr->value());
								if (p < Postures::NUM_POSTURES)
								{
									// Found it
									recognizerRefs.push_back(createPostureRecognizer(p));
									recognizerRefs.back()->m_ignoreOnTrackingError = ignoreOnTrackingError;
									if (minConf >= 0)
										recognizerRefs.back()->m_minConfidence = minConf;
									else if (globalMinConfidence >= 0)	
										// Only for predefined recognizers, global confidence is allowed to overwrite
										recognizerRefs.back()->m_minConfidence = globalMinConfidence;
									if (filterA)
										recognizerRefs.back()->m_useFilteredData = useFilteredData;
									else if (globalUseFilteredData)
										recognizerRefs.back()->m_useFilteredData = true;
								}
								else
								{
									// Finally not found
									Fubi_logErr("XML_Error - Unknown reference \"%s\" in \"%s\"!\n", attr->value(), rec->getName().c_str());
								}
							}
						}
					}

				}
			}

			std::vector<IGestureRecognizer*> notRecognizerRefs;
			rapidxml::xml_node<>* notRecRefNode;
			for(notRecRefNode = stateNode->first_node("NotRecognizer"); notRecRefNode; notRecRefNode = notRecRefNode->next_sibling("NotRecognizer"))
			{
				std::string name;
				rapidxml::xml_attribute<>* attr = notRecRefNode->first_attribute("name");
				if (attr)
				{
					// Default for not recognizers is true, as with a tracking error there is also no recognition
					bool ignoreOnTrackingError = true;
					rapidxml::xml_attribute<>* attr1 = notRecRefNode->first_attribute("ignoreOnTrackingError");
					if (attr1)
					{
						std::string lowerValue = removeWhiteSpacesAndToLower(attr1->value());
						ignoreOnTrackingError = lowerValue != "0" && lowerValue != "false";
					}

					float minConf = -1.0f;
					rapidxml::xml_attribute<>* minConfA = notRecRefNode->first_attribute("minConfidence");
					if (minConfA)
						minConf = (float)atof(minConfA->value());

					bool useFilteredData = false;
					rapidxml::xml_attribute<>* filterA = notRecRefNode->first_attribute("useFilteredData");
					if (filterA)
						useFilteredData = strcmp(filterA->value(), "false") != 0 && strcmp(filterA->value(), "0") != 0;

					int index = getRecognizerIndex(attr->value(), recognizerContainer);
					if (index > -1)
					{
						// found it
						notRecognizerRefs.push_back(recognizerContainer[index].second->clone());
						notRecognizerRefs.back()->m_ignoreOnTrackingError = ignoreOnTrackingError;
						if (minConf >= 0)
							notRecognizerRefs.back()->m_minConfidence = minConf;
						if (filterA)
							notRecognizerRefs.back()->m_useFilteredData = useFilteredData;
					}
					else
					{
						index = getRecognizerIndex(attr->value(), hiddenRecognizerContainer);
						if (index > -1)
						{
							// found it
							notRecognizerRefs.push_back(hiddenRecognizerContainer[index].second->clone());
							notRecognizerRefs.back()->m_ignoreOnTrackingError = ignoreOnTrackingError;
							if (minConf >= 0)
								notRecognizerRefs.back()->m_minConfidence = minConf;
							if (filterA)
								notRecognizerRefs.back()->m_useFilteredData = useFilteredData;
						}
						else
						{
							index = atoi(attr->value());
							if ((index > 0 || (index == 0 && attr->value()[0] == '0')) && (unsigned) index < recognizerContainer.size())
							{
								// name in fact represents the index of a recognizer
								notRecognizerRefs.push_back(recognizerContainer[index].second->clone());
								notRecognizerRefs.back()->m_ignoreOnTrackingError = ignoreOnTrackingError;
								if (minConf >= 0)
									notRecognizerRefs.back()->m_minConfidence = minConf;
								if (filterA)
									notRecognizerRefs.back()->m_useFilteredData = useFilteredData;
							}
							else // last option: name belongs to a predefined gesture
							{
								Postures::Posture p = getPostureID(attr->value());
								if (p < Postures::NUM_POSTURES)
								{
									// Found it
									notRecognizerRefs.push_back(createPostureRecognizer(p));
									notRecognizerRefs.back()->m_ignoreOnTrackingError = ignoreOnTrackingError;
									if (minConf >= 0)
										notRecognizerRefs.back()->m_minConfidence = minConf;
									else if (globalMinConfidence >= 0)	
										// Only for predefined recognizers, global confidence is allowed to overwrite
										notRecognizerRefs.back()->m_minConfidence = globalMinConfidence;

									if (filterA)
										notRecognizerRefs.back()->m_useFilteredData = useFilteredData;
									else if (globalUseFilteredData)
										notRecognizerRefs.back()->m_useFilteredData = true;
								}
								else
								{
									// Finally not found
									Fubi_logErr("XML_Error - Unknown reference \"%s\" in \"%s\"!\n", attr->value(), rec->getName().c_str());
								}
							}
						}
					}

				}
			}

			// Now check for alternative recognizers in the same way
			std::vector<IGestureRecognizer*> alternativeRecognizerRefs;
			std::vector<IGestureRecognizer*> alternativeNotRecognizerRefs;
			rapidxml::xml_node<>* alternativesNode = stateNode->first_node("AlternativeRecognizers");
			if (alternativesNode)
			{
				rapidxml::xml_node<>* alternativeRecRefNode;
				for(alternativeRecRefNode = alternativesNode->first_node("Recognizer"); alternativeRecRefNode; alternativeRecRefNode = alternativeRecRefNode->next_sibling("Recognizer"))
				{
					std::string name;
					rapidxml::xml_attribute<>* attr = alternativeRecRefNode->first_attribute("name");
					if (attr)
					{
						bool ignoreOnTrackingError = false;
						rapidxml::xml_attribute<>* attr1 = alternativeRecRefNode->first_attribute("ignoreOnTrackingError");
						if (attr1)
						{
							std::string lowerValue = removeWhiteSpacesAndToLower(attr1->value());
							ignoreOnTrackingError = lowerValue != "0" && lowerValue != "false";
						}

						float minConf = -1.0f;
						rapidxml::xml_attribute<>* minConfA = alternativeRecRefNode->first_attribute("minConfidence");
						if (minConfA)
							minConf = (float)atof(minConfA->value());

						bool useFilteredData = false;
						rapidxml::xml_attribute<>* filterA = alternativeRecRefNode->first_attribute("useFilteredData");
						if (filterA)
							useFilteredData = strcmp(filterA->value(), "false") != 0 && strcmp(filterA->value(), "0") != 0;

						int index = getRecognizerIndex(attr->value(), recognizerContainer);
						if (index > -1)
						{
							// found it
							alternativeRecognizerRefs.push_back(recognizerContainer[index].second->clone());
							alternativeRecognizerRefs.back()->m_ignoreOnTrackingError = ignoreOnTrackingError;
							if (minConf >= 0)
								alternativeRecognizerRefs.back()->m_minConfidence = minConf;
							if (filterA)
								alternativeRecognizerRefs.back()->m_useFilteredData = useFilteredData;
						}
						else
						{
							index = getRecognizerIndex(attr->value(), hiddenRecognizerContainer);
							if (index > -1)
							{
								// found it
								alternativeRecognizerRefs.push_back(hiddenRecognizerContainer[index].second->clone());
								alternativeRecognizerRefs.back()->m_ignoreOnTrackingError = ignoreOnTrackingError;
								if (minConf >= 0)
									alternativeRecognizerRefs.back()->m_minConfidence = minConf;
								if (filterA)
									alternativeRecognizerRefs.back()->m_useFilteredData = useFilteredData;
							}
							else
							{
								index = atoi(attr->value());
								if ((index > 0 || (index == 0 && attr->value()[0] == '0')) && (unsigned) index < recognizerContainer.size())
								{
									// name in fact represents the index of a recognizer
									alternativeRecognizerRefs.push_back(recognizerContainer[index].second->clone());
									alternativeRecognizerRefs.back()->m_ignoreOnTrackingError = ignoreOnTrackingError;
									if (minConf >= 0)
										alternativeRecognizerRefs.back()->m_minConfidence = minConf;
									if (filterA)
										alternativeRecognizerRefs.back()->m_useFilteredData = useFilteredData;
								}
								else // last option: name belongs to a predefined gesture
								{
									Postures::Posture p = getPostureID(attr->value());
									if (p < Postures::NUM_POSTURES)
									{
										// Found it
										alternativeRecognizerRefs.push_back(createPostureRecognizer(p));
										alternativeRecognizerRefs.back()->m_ignoreOnTrackingError = ignoreOnTrackingError;
										if (minConf >= 0)
											alternativeRecognizerRefs.back()->m_minConfidence = minConf;
										else if (globalMinConfidence >= 0)	
											// Only for predefined recognizers, global confidence is allowed to overwrite
											alternativeRecognizerRefs.back()->m_minConfidence = globalMinConfidence;
										if (filterA)
											alternativeRecognizerRefs.back()->m_useFilteredData = useFilteredData;
										else if (globalUseFilteredData)
											alternativeRecognizerRefs.back()->m_useFilteredData = true;
									}
									else
									{
										// Finally not found
										Fubi_logErr("XML_Error - Unknown reference \"%s\" in \"%s\"!\n", attr->value(), rec->getName().c_str());
									}
								}
							}
						}

					}
				}

				rapidxml::xml_node<>* alternativeNotRecRefNode;
				for(alternativeNotRecRefNode = alternativesNode->first_node("NotRecognizer"); alternativeNotRecRefNode; alternativeNotRecRefNode = alternativeNotRecRefNode->next_sibling("NotRecognizer"))
				{
					std::string name;
					rapidxml::xml_attribute<>* attr = alternativeNotRecRefNode->first_attribute("name");
					if (attr)
					{
						// Default for not recognizers is true, as with a tracking error there is also no recognition
						bool ignoreOnTrackingError = true;
						rapidxml::xml_attribute<>* attr1 = alternativeNotRecRefNode->first_attribute("ignoreOnTrackingError");
						if (attr1)
						{
							std::string lowerValue = removeWhiteSpacesAndToLower(attr1->value());
							ignoreOnTrackingError = lowerValue != "0" && lowerValue != "false";
						}

						float minConf = -1.0f;
						rapidxml::xml_attribute<>* minConfA = alternativeNotRecRefNode->first_attribute("minConfidence");
						if (minConfA)
							minConf = (float)atof(minConfA->value());

						bool useFilteredData = false;
						rapidxml::xml_attribute<>* filterA = alternativeNotRecRefNode->first_attribute("useFilteredData");
						if (filterA)
							useFilteredData = strcmp(filterA->value(), "false") != 0 && strcmp(filterA->value(), "0") != 0;

						int index = getRecognizerIndex(attr->value(), recognizerContainer);
						if (index > -1)
						{
							// found it
							alternativeNotRecognizerRefs.push_back(recognizerContainer[index].second->clone());
							alternativeNotRecognizerRefs.back()->m_ignoreOnTrackingError = ignoreOnTrackingError;
							if (minConf >= 0)
								alternativeNotRecognizerRefs.back()->m_minConfidence = minConf;
							if (filterA)
								alternativeNotRecognizerRefs.back()->m_useFilteredData = useFilteredData;
						}
						else
						{
							index = getRecognizerIndex(attr->value(), hiddenRecognizerContainer);
							if (index > -1)
							{
								// found it
								alternativeNotRecognizerRefs.push_back(hiddenRecognizerContainer[index].second->clone());
								alternativeNotRecognizerRefs.back()->m_ignoreOnTrackingError = ignoreOnTrackingError;
								if (minConf >= 0)
									alternativeNotRecognizerRefs.back()->m_minConfidence = minConf;
								if (filterA)
									alternativeNotRecognizerRefs.back()->m_useFilteredData = useFilteredData;
							}
							else
							{
								index = atoi(attr->value());
								if ((index > 0 || (index == 0 && attr->value()[0] == '0')) && (unsigned) index < recognizerContainer.size())
								{
									// name in fact represents the index of a recognizer
									alternativeNotRecognizerRefs.push_back(recognizerContainer[index].second->clone());
									alternativeNotRecognizerRefs.back()->m_ignoreOnTrackingError = ignoreOnTrackingError;
									if (minConf >= 0)
										alternativeNotRecognizerRefs.back()->m_minConfidence = minConf;
									if (filterA)
										alternativeNotRecognizerRefs.back()->m_useFilteredData = useFilteredData;
								}
								else // last option: name belongs to a predefined gesture
								{
									Postures::Posture p = getPostureID(attr->value());
									if (p < Postures::NUM_POSTURES)
									{
										// Found it
										alternativeNotRecognizerRefs.push_back(createPostureRecognizer(p));
										alternativeNotRecognizerRefs.back()->m_ignoreOnTrackingError = ignoreOnTrackingError;
										if (minConf >= 0)
											alternativeNotRecognizerRefs.back()->m_minConfidence = minConf;
										else if (globalMinConfidence >= 0)	
											// Only for predefined recognizers, global confidence is allowed to overwrite
											alternativeNotRecognizerRefs.back()->m_minConfidence = globalMinConfidence;
										if (filterA)
											alternativeNotRecognizerRefs.back()->m_useFilteredData = useFilteredData;
										else if (globalUseFilteredData)
											alternativeNotRecognizerRefs.back()->m_useFilteredData = true;
									}
									else
									{
										// Finally not found
										Fubi_logErr("XML_Error - Unknown reference \"%s\" in \"%s\"!\n", attr->value(), rec->getName().c_str());
									}
								}
							}
						}

					}
				}
			}

			if (recognizerRefs.size() > 0 || notRecognizerRefs.size() > 0)
			{
				// Finally load any meta data
				std::map<std::string, std::string> metaInfo;
				rapidxml::xml_node<>* metaInfoNode = stateNode->first_node("METAINFO");
				if (metaInfoNode)
				{
					rapidxml::xml_node<>* metaProperty;
					for(metaProperty = metaInfoNode->first_node("Property"); metaProperty; metaProperty = metaProperty->next_sibling("Property"))
					{
						std::string name;
						rapidxml::xml_attribute<>* attr = metaProperty->first_attribute("name");
						if (attr && attr->value() != 0x0)
						{
							name = attr->value();
							attr = metaProperty->first_attribute("value");
							if (attr &&  attr->value() != 0x0)
							{
								metaInfo[name] = attr->value();
							}
						}
					}
				}


				// Add state to the recognizer
				rec->addState(recognizerRefs, notRecognizerRefs, minDuration, maxDuration, timeForTransition, maxInterruption,
					noInterrruptionBeforeMinDuration, alternativeRecognizerRefs, alternativeNotRecognizerRefs, restartOnFail, metaInfo);
			}
			else
			{
				// No recognizers in this state
				Fubi_logInfo("FubiXMLParser: XML_Error - No references in state %d of rec \"%s\"!\n", stateNum, rec->getName().c_str());
			}
		}

		if (rec->getNumStates() > 0)
		{
			success = true;
			// Add the recognizer to the templates
			combinationRecognizerContainer.push_back(std::pair<string, CombinationRecognizer*>(name, rec));
		}
		else
			delete rec; // not passed to fubi control so we have to delete it ourselves
	}

	return success;
}

int FubiXMLParser::getRecognizerIndex(const std::string& name,
	const vector<pair<string, IGestureRecognizer*> >& container)
{
	if (name.length() > 0)
	{
        vector<pair<string, IGestureRecognizer*> >::const_iterator iter = container.begin();
        const vector<pair<string, IGestureRecognizer*> >::const_iterator end = container.end();
		for (int i = 0; iter != end; ++iter, ++i)
		{
			if (name == iter->first)
				return i;
		}
	}
	return -1;
}