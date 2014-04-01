#include "Fubi.h"

#include <iostream>
#include <string>
#include <sstream>
#include <queue>

#ifdef __APPLE__
#include <glut.h>
#else
#include <GL/glut.h>
#endif

#include <FubiUtils.h>

#if defined ( WIN32 ) || defined( _WINDOWS )
#include <Windows.h>
#endif

using namespace Fubi;

// Some additional OpenGL defines
#define GL_GENERATE_MIPMAP_SGIS           0x8191
#define GL_GENERATE_MIPMAP_HINT_SGIS      0x8192
#define GL_BGRA                           0x80E1


// Some global variables for the application
unsigned char* g_depthData = 0x0;
unsigned char* g_rgbData = 0x0;
unsigned char* g_irData = 0x0;

int dWidth= 0, dHeight= 0, rgbWidth= 0, rgbHeight= 0, irWidth = 0, irHeight = 0;

bool g_showRGBImage = false;
bool g_showIRImage = false;

short g_showInfo = 0;

bool g_showFingerCounts = false;
bool g_useOldFingerCounts = false;

bool g_currentPostures[Postures::NUM_POSTURES];
std::vector<bool> g_currentUserDefinedRecognizers;
std::vector<bool> g_currentUserDefinedHandRecognizers;
short g_recognitionOutputMode = 0;
const short g_numOutputmodes = 5;
bool g_takePictures = false;
bool g_exitNextFrame = false;

// Function called each frame for all tracked users/hands
void checkPostures(unsigned int targetID)
{
	switch(g_recognitionOutputMode)
	{
	case 4: // Special: pointing gestures (categorie 1)
		{
			if (recognizeGestureOn(Postures::POINTING_RIGHT, targetID) == RecognitionResult::RECOGNIZED)
			{
				// User is pointing somewhere
				FubiUser* user = getUser(targetID);
				// Get pointing direction
				const Vec3f& rightHand = user->m_currentTrackingData->jointPositions[SkeletonJoint::RIGHT_HAND].m_position;
				const Vec3f& rightShoulder = user->m_currentTrackingData->jointPositions[SkeletonJoint::RIGHT_SHOULDER].m_position;
				Vec3f dir = rightHand - rightShoulder;
				printf("User is pointing from (%.0f,%.0f,%.0f) to dir (%.0f,%.0f,%.0f)\n", rightShoulder.x, rightShoulder.y, rightShoulder.z, dir.x, dir.y, dir.z);
			}

			break;
		}
	case 3: // Call recognizers for all postures (categorie 1)
		{
			for (unsigned int i= 0; i < Postures::NUM_POSTURES; ++i)
			{
				Postures::Posture p = (Postures::Posture) i;
				if (recognizeGestureOn(p, targetID) == RecognitionResult::RECOGNIZED)
				{
					if (!g_currentPostures[i])
						printf("User %d - Posture Start: %s! -->\n", targetID, getPostureName(p));
					g_currentPostures[i] = true;
				}
				else if (g_currentPostures[i])
				{
					printf("--> User %d - Posture Finished: %s!\n", targetID, getPostureName(p));
					g_currentPostures[i] = false;
				}
			}
			break;
		}
	case 2: // Combinations (categorie 3)
		{
			for (unsigned int i= 0; i < Combinations::NUM_COMBINATIONS; ++i)
			{
				Combinations::Combination p = (Combinations::Combination) i;
				std::vector<FubiUser::TrackingData> recInfo;
				if (getCombinationRecognitionProgressOn(p, targetID, &recInfo) == RecognitionResult::RECOGNIZED)
				{
					Vec3f handMov;
					float vel = 0;
					if (recInfo.size() >= 3)
					{
						FubiUser::TrackingData& lastState = recInfo.back();
						FubiUser::TrackingData& preState = recInfo[0];

						handMov = lastState.jointPositions[SkeletonJoint::RIGHT_HAND].m_position - preState.jointPositions[SkeletonJoint::RIGHT_HAND].m_position;
						handMov *= 0.001f; // Convert to meter
						vel = handMov.length() / (lastState.timeStamp - preState.timeStamp);
					}
					printf("User %d -- Combination Succeeded: %s! Vec:%.2f|%.2f|%.2f Vel:%.2f\n", targetID, 
						getCombinationName(p), handMov.x, handMov.y, handMov.z, vel);
				}
			}
			break;
		}
	case 1: // User defined combinations
		{
			for (unsigned int i= 0; i < getNumUserDefinedCombinationRecognizers(); ++i)
			{
				if (getCombinationRecognitionProgressOn(getUserDefinedCombinationRecognizerName(i), targetID) == RecognitionResult::RECOGNIZED)
				{
					printf("User %d -- User Defined Combination Succeeded: %s!\n", targetID, 
						getUserDefinedCombinationRecognizerName(i));
				}
				if (getCombinationRecognitionProgressOnHand(getUserDefinedCombinationRecognizerName(i), targetID) == RecognitionResult::RECOGNIZED)
				{
					printf("Hand %d -- User Defined Combination Succeeded: %s!\n", targetID, 
						getUserDefinedCombinationRecognizerName(i));
				}
			}
			break;
		}
	case 0: // User defined joint relations and linear gestures (categorie 1 + 2)
		{
			unsigned int numRels = getNumUserDefinedRecognizers();
			if (g_currentUserDefinedRecognizers.size() != numRels)
			{
				g_currentUserDefinedRecognizers.resize(numRels, false);
			}
			if (g_currentUserDefinedHandRecognizers.size() != numRels)
			{
				g_currentUserDefinedHandRecognizers.resize(numRels, false);
			}
			for (unsigned int i=0; i < getNumUserDefinedRecognizers(); ++i)
			{
				if (recognizeGestureOn(i, targetID)== RecognitionResult::RECOGNIZED)
				{				
					if (!g_currentUserDefinedRecognizers[i])
						printf("User %d - User defined Rec START: %d %s -->\n", targetID, i, getUserDefinedRecognizerName(i));
					g_currentUserDefinedRecognizers[i] = true;
				}
				else if (g_currentUserDefinedRecognizers[i])
				{
					printf("--> User %d - User defined Rec FINISHED: %d %s!\n", targetID, i, getUserDefinedRecognizerName(i));
					g_currentUserDefinedRecognizers[i] = false;
				}
				if (recognizeGestureOnHand(getUserDefinedRecognizerName(i), targetID)== RecognitionResult::RECOGNIZED)
				{				
					if (!g_currentUserDefinedHandRecognizers[i])
						printf("Hand %d - User defined Rec START: %d %s -->\n", targetID, i, getUserDefinedRecognizerName(i));
					g_currentUserDefinedHandRecognizers[i] = true;
				}
				else if (g_currentUserDefinedHandRecognizers[i])
				{
					printf("--> Hand %d - User defined Rec FINISHED: %d %s!\n", targetID, i, getUserDefinedRecognizerName(i));
					g_currentUserDefinedHandRecognizers[i] = false;
				}

			}
			break;
		}
	}
}

void glutIdle (void)
{
	// Display the frame
	glutPostRedisplay();
}

// The glut update functions called every frame
void glutDisplay (void)
{
	if (g_exitNextFrame)
	{
		release();
		exit (0);
	}

	// Update the sensor	
	updateSensor();

	ImageType::Type type = ImageType::Depth;
	ImageNumChannels::Channel numChannels = ImageNumChannels::C4;
	unsigned char* buffer = g_depthData;

	if (Fubi::getCurrentSensorType() == SensorType::NONE)
	{
		type = ImageType::Blank;
		memset(buffer, 0, dWidth*dHeight*4);
	}
	else if (g_showRGBImage)
	{
		buffer = g_rgbData;
		type = ImageType::Color;
		numChannels = ImageNumChannels::C3;
	}
	else if (g_showIRImage)
	{
		buffer = g_irData;
		type = ImageType::IR;
	}
	unsigned int options = RenderOptions::None;
	DepthImageModification::Modification mod = DepthImageModification::UseHistogram;
	if (g_showInfo == 0)
		options = RenderOptions::Shapes | RenderOptions::UserCaptions | RenderOptions::Skeletons | RenderOptions::FingerShapes | RenderOptions::Background | RenderOptions::DetailedFaceShapes | RenderOptions::UseFilteredValues | RenderOptions::FingerShapes;
	else if (g_showInfo == 1)
		options = RenderOptions::Shapes | RenderOptions::LocalOrientCaptions | RenderOptions::Skeletons;
	else if (g_showInfo == 2)
		mod = DepthImageModification::ConvertToRGB;
	else if (g_showInfo == 3)
		mod = DepthImageModification::StretchValueRange;

	getImage(buffer, type, numChannels, ImageDepth::D8, options, RenderOptions::ALL_JOINTS, mod);

	// Clear the OpenGL buffers
	glClear (GL_COLOR_BUFFER_BIT | GL_DEPTH_BUFFER_BIT);

	// Setup the OpenGL viewpoint
	glMatrixMode(GL_PROJECTION);
	glPushMatrix();
	glLoadIdentity();
	glOrtho(0, (double)dWidth, (double)dHeight, 0, -1.0, 1.0);


	// Create the OpenGL texture map
	glEnable(GL_TEXTURE_2D);
	glTexParameteri(GL_TEXTURE_2D, GL_GENERATE_MIPMAP_SGIS, GL_TRUE);
	glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MIN_FILTER, GL_LINEAR_MIPMAP_LINEAR);
	glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MAG_FILTER, GL_LINEAR);
	if (g_showRGBImage)
		glTexImage2D(GL_TEXTURE_2D, 0, GL_RGB, rgbWidth, rgbHeight, 0, GL_RGB, GL_UNSIGNED_BYTE, g_rgbData);
	else if (g_showIRImage)
		glTexImage2D(GL_TEXTURE_2D, 0, GL_RGB, irWidth, irHeight, 0, GL_BGRA, GL_UNSIGNED_BYTE, g_irData);
	else
		glTexImage2D(GL_TEXTURE_2D, 0, GL_RGB, dWidth, dHeight, 0, GL_BGRA, GL_UNSIGNED_BYTE, g_depthData);

	// Display the OpenGL texture map
	glColor4f(1,1,1,1);

	glBegin(GL_QUADS);

	// upper left
	glTexCoord2f(0, 0);
	glVertex2f(0, 0);
	// upper right
	glTexCoord2f(1.0f, 0);
	glVertex2f((float)dWidth, 0);
	// bottom right
	glTexCoord2f(1.0f, 1.0f);
	glVertex2f((float)dWidth, (float)dHeight);
	// bottom left
	glTexCoord2f(0, 1.0f);
	glVertex2f(0, (float)dHeight);

	glEnd();
	glDisable(GL_TEXTURE_2D);

	// Check closest user's gestures
	unsigned int closestID = getClosestUserID();
	if (closestID > 0)
	{
		checkPostures(closestID);
		if (g_showFingerCounts)
		{
			/*static int pause = 0;
			if (pause == 0)
			{
				printf("User %d finger count: left=%d, right=%d\n", closestID, getFingerCount(closestID, true, g_useOldFingerCounts), getFingerCount(closestID, false, g_useOldFingerCounts));
			}
			pause = (pause + 1) % 20;*/
			printf("User %d finger count right=%d\n", closestID, getFingerCount(closestID, false, g_useOldFingerCounts));
		}
	}

	unsigned short numHands = getNumHands();
	if (numHands > 0)
	{
		checkPostures(getHandID(0));
	}

	// Swap the OpenGL display buffers
	glutSwapBuffers();

	if (g_takePictures)
	{
		static int pause = 0;
		if (pause == 0)
		{
			static int num = 0;
			std::stringstream str;
			/*str << "rgbImage" << num << ".jpg";
			saveImage(str.str().c_str(), 95, ImageType::Color, ImageNumChannels::C3, ImageDepth::D8, RenderOptions::None, DepthImageModification::Raw, 1, SkeletonJoint::HEAD);
			str.str("");
			str << "depthImage" << num << ".png";
			saveImage(str.str().c_str(), 95, ImageType::Depth, ImageNumChannels::C1, ImageDepth::D16, RenderOptions::None, DepthImageModification::StretchValueRange);
			str.str("");*/
			str << "trackingImage" << num << ".png";
			saveImage(str.str().c_str(), 95, ImageType::Depth, ImageNumChannels::C3, ImageDepth::D8, RenderOptions::Shapes | RenderOptions::Skeletons | RenderOptions::DetailedFaceShapes);
			num++;
		}
		pause = (pause + 1) % 30;
	}
}

void startNextFingerSensor()
{
	FingerSensorType::Type type = Fubi::getCurrentFingerSensorType();
	bool success = false;
	while (!success)
	{
		if (type == FingerSensorType::NONE)
			type = FingerSensorType::LEAP;
		else if(type == FingerSensorType::LEAP)
			type = FingerSensorType::NONE;

		// init finger sensor, but you should actually set you individual offset position
		success = Fubi::initFingerSensor(type, 0, -600.0f, 200.0f);

		if (type == SensorType::NONE)
			break;	// None should always be successful so we ensure termination of this loop
	}
}

void startNextSensor()
{
	SensorType::Type type = Fubi::getCurrentSensorType();
	bool success = false;
	while (!success)
	{
		if (type == SensorType::NONE)
			type = SensorType::OPENNI2;
		else if (type == SensorType::OPENNI2)
			type = SensorType::OPENNI1;
		else if (type == SensorType::OPENNI1)
			type = SensorType::KINECTSDK;
		else if (type == SensorType::KINECTSDK)
			type = SensorType::NONE;
			
		if (Fubi::isInitialized())
			success = Fubi::switchSensor(SensorOptions(StreamOptions(), StreamOptions(), StreamOptions(-1, -1, -1), type, SkeletonTrackingProfile::ALL, true, false));
		else
			success = Fubi::init(SensorOptions(StreamOptions(), StreamOptions(), StreamOptions(-1, -1, -1), type, SkeletonTrackingProfile::ALL, true, false));

		if (success)
		{
			int w, h;
			if (type == SensorType::NONE && (dWidth != 640 || dHeight != 640))
			{
				w = 640;
				h = 480;
			}
			else
				getDepthResolution(w, h);
			if (w > 0 && h > 0 && (w != dWidth || h != dHeight))
			{
				dWidth = w; dHeight = h;
				getRgbResolution(rgbWidth, rgbHeight);
				getIRResolution(irWidth, irHeight);

				delete[] g_depthData;
				delete[] g_rgbData;
				delete[] g_irData;
				g_depthData = new unsigned char[dWidth*dHeight*4];
				if ( rgbWidth > 0 && rgbHeight > 0)
					g_rgbData = new unsigned char[rgbWidth*rgbHeight*3];
				if (irWidth > 0 && irHeight > 0)
					g_irData = new unsigned char[irWidth*irHeight*4];
				if (dWidth > 0 && dHeight > 0)
				{
					glutReshapeWindow(dWidth, dHeight);
					#if defined ( WIN32 ) || defined( _WINDOWS )
						SetWindowPos( GetConsoleWindow(), HWND_TOP, dWidth+10, 0, 0, 0,
											SWP_NOOWNERZORDER | SWP_NOSIZE | SWP_NOZORDER );
					#endif
				}
			}
		}

		if (type == SensorType::NONE)
			break;	// None should always be successful so we ensure termination of this loop
	}
}

// Glut keyboards callback
void glutKeyboard (unsigned char key, int x, int y)
{
	//printf("key: %d\n", key);
	switch (key)
	{
	case 27: //ESC
		g_exitNextFrame = true;
		break;
	case ' ':
		{
			g_recognitionOutputMode = (g_recognitionOutputMode+1) % g_numOutputmodes;
			char* modeName = "Unknown";
			switch (g_recognitionOutputMode)
			{
				case 0: 
					modeName = "User defined joint relations, orientation, and linear gestures (categorie 1 + 2)";
					break;
				case 1: 
					modeName = "User defined posture combinations (categorie 3";
					break;
				case 2: 
					modeName = "Posture combinations (categorie 3)";
					break;
				case 3: 
					modeName = "Static Postures::Posture (categorie 1)";
					break;
				case 4: 
					modeName = "Special: Pointing gestures (categorie 1)";
					break;
			}
			std::cout << "Check recognizer mode: " << g_recognitionOutputMode << "- " << modeName << std::endl;
		}
		break;
	case 'p':
		{
			g_takePictures = !g_takePictures;
		}
		break;
	case 'r':
		{
			g_showRGBImage = !g_showRGBImage;
		}
		break;
	case 'i':
		{
			g_showIRImage = !g_showIRImage;

		}
		break;
	case 'f':
		{
			g_showFingerCounts = !g_showFingerCounts;
		}
		break;
	case 'c':
		{
			g_useOldFingerCounts = !g_useOldFingerCounts;
		}
		break;
	case 't':
		{
			g_showInfo = (g_showInfo+1) % 4;
		}
		break;
	case 's':
		{
			startNextSensor();
		}
		break;
	case 'd':
		startNextFingerSensor();
		break;
	case 9: //TAB
		{
			// Reload recognizers from xml
			clearUserDefinedRecognizers();
			if (loadRecognizersFromXML("SampleRecognizers.xml"))
			{
				printf("Succesfully reloaded recognizers xml!\n");
			}
		}
	}
}

int main(int argc, char ** argv)
{
	// init current posture as not recognized
	memset(g_currentPostures, 0, sizeof(g_currentPostures));

	// OpenGL init
	glutInit(&argc, argv);
	glutInitDisplayMode(GLUT_RGB | GLUT_DOUBLE | GLUT_DEPTH);
	glutInitWindowSize(50, 50);
	glutCreateWindow ("FUBI - Recognizer OpenGL test");
	glutKeyboardFunc(glutKeyboard);
	glutDisplayFunc(glutDisplay);
	glutIdleFunc(glutIdle);
	glDisable(GL_DEPTH_TEST);
	glEnable(GL_TEXTURE_2D);
	
	// Fubi init
	// Directly trying to start the next available sensor
	startNextSensor();
	// All known combination recognizers will be started automatically for new users
	setAutoStartCombinationRecognition(true);
	// Loading the sample recognizer definitions
	loadRecognizersFromXML("SampleRecognizers.xml");
	loadRecognizersFromXML("SampleFingerRecognizers.xml");

	// Start the glut main loop, that constantly calls glutDisplay
	glutMainLoop();

	// Until ESC is pressed
	// Now release Fubi
	release();
	// And all allocated buffers
	delete[] g_depthData;
	delete[] g_rgbData;
	delete[] g_irData;

	return 0;
}
