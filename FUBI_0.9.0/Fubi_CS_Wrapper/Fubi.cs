// ****************************************************************************************
//
// Fubi CS Wrapper
// ---------------------------------------------------------
// Copyright (C) 2010-2011 Felix Kistler 
// 
// This software is distributed under the terms of the Eclipse Public License v1.0.
// A copy of the license may be obtained at: http://www.eclipse.org/org/documents/epl-v10.html
// 
// ****************************************************************************************

using System;
using System.Runtime.InteropServices;

/** \file Fubi.cs 
 * \brief Contains the Fubi CS wrapper
*/ 

/**
 * \namespace FubiNET
 *
 * \brief The FubiNET namespace holds the Fubi class that wraps the C++ functions of Fubi
 *
 */
namespace FubiNET
{
    /**
     *
     * \brief The Fubi class wraps the C++ functions of Fubi. Note: documentation not complete! For more details have a look at the C++ docu
     *
     */
    public class Fubi
    {
        /** \addtogroup FUBICSHARP FUBI C# API
	     * All the C# API functions (subset of the C++ functions + some additional ones)
         * Note: documentation not complete! For more details have a look at the C++ docu
	     * 
	     * @{
	     */

        public static bool init(string openniXmlconfig, FubiUtils.SkeletonProfile profile = FubiUtils.SkeletonProfile.ALL,
            float filterMinCutOffFrequency = 1.0f, float filterVelocityCutOffFrequency = 1.0f, float filterCutOffSlope = 0.007f,
            bool mirrorStreams = true, bool registerStreams = true)
        {
            bool ret = true;
            if (!isInitialized())
            {
                IntPtr openNiXmlPtr = Marshal.StringToHGlobalAnsi(openniXmlconfig);
                ret = FubiInternal.init(openNiXmlPtr, profile, filterMinCutOffFrequency, filterVelocityCutOffFrequency, filterCutOffSlope, mirrorStreams, registerStreams);
                Marshal.FreeHGlobal(openNiXmlPtr);
            }
            return ret;
        }

        public static bool init(FubiUtils.SensorOptions sensorOptions, FubiUtils.FilterOptions filterOptions)
        {
            bool ret = true;
            if (!isInitialized())
            {
                ret = FubiInternal.init(sensorOptions.m_depthOptions.m_width, sensorOptions.m_depthOptions.m_height, sensorOptions.m_depthOptions.m_fps,
                    sensorOptions.m_rgbOptions.m_width, sensorOptions.m_rgbOptions.m_height, sensorOptions.m_rgbOptions.m_fps,
                    sensorOptions.m_irOptions.m_width, sensorOptions.m_irOptions.m_height, sensorOptions.m_irOptions.m_fps,
                    sensorOptions.m_type,
                    sensorOptions.m_trackingProfile,
                    filterOptions.m_filterMinCutOffFrequency, filterOptions.m_filterVelocityCutOffFrequency, filterOptions.m_filterCutOffSlope,
                    sensorOptions.m_mirrorStreams, sensorOptions.m_registerStreams);
            }
            return ret;
        }

        public static bool switchSensor(FubiUtils.SensorOptions options)
        {
            return FubiInternal.switchSensor(options.m_type, options.m_depthOptions.m_width, options.m_depthOptions.m_height, options.m_depthOptions.m_fps,
                    options.m_rgbOptions.m_width, options.m_rgbOptions.m_height, options.m_rgbOptions.m_fps,
                    options.m_irOptions.m_width, options.m_irOptions.m_height, options.m_irOptions.m_fps,
                    options.m_trackingProfile,
                    options.m_mirrorStreams, options.m_registerStreams);
        }

        public static int getAvailableSensors()
        {
            return FubiInternal.getAvailableSensorTypes();
        }

        public static FubiUtils.SensorType getCurrentSensorType()
        {
            return FubiInternal.getCurrentSensorType();
        }

        public static void release()
        {
            FubiInternal.release();
        }

        public static void updateSensor()
        {
            FubiInternal.updateSensor();
        }

        public static void getImage(byte[] outputImage, FubiUtils.ImageType type, FubiUtils.ImageNumChannels numChannels, FubiUtils.ImageDepth depth,
            int renderOptions = (int)FubiUtils.RenderOptions.Default,
            int jointsToRender = (int)FubiUtils.JointsToRender.ALL_JOINTS,
            FubiUtils.DepthImageModification depthModifications = FubiUtils.DepthImageModification.UseHistogram,
            uint userId = 0, FubiUtils.SkeletonJoint jointOfInterest = FubiUtils.SkeletonJoint.NUM_JOINTS, bool moveCroppedToUpperLeft = false)
        {
            if (outputImage != null)
            {
                GCHandle h = GCHandle.Alloc(outputImage, GCHandleType.Pinned);
                FubiInternal.getImage(h.AddrOfPinnedObject(), type, numChannels, depth, renderOptions, jointsToRender, depthModifications, userId, jointOfInterest, moveCroppedToUpperLeft);
                h.Free();

                return;
            }
        }


        public static FubiUtils.RecognitionResult recognizeGestureOn(FubiPredefinedGestures.Postures postureID, UInt32 userID)
        {
            return FubiInternal.recognizeGestureOn(postureID, userID);
        }

        public static FubiUtils.RecognitionResult recognizeGestureOn(UInt32 recognizerIndex, UInt32 userID)
        {
            return FubiInternal.recognizeGestureOn(recognizerIndex, userID);
        }
        public static FubiUtils.RecognitionResult recognizeGestureOn(UInt32 recognizerIndex, UInt32 userID, out FubiUtils.RecognitionCorrectionHint correctionHint)
        {
            FubiUtils.RecognitionCorrectionHint hint = new FubiUtils.RecognitionCorrectionHint();
            FubiUtils.RecognitionResult res = FubiInternal.recognizeGestureOn(recognizerIndex, userID, hint);
            correctionHint = hint;
            return res;
        }

        public static UInt32 getUserID(UInt32 index)
        {
            return FubiInternal.getUserID(index);
        }

        public static void setAutoStartCombinationRecognition(bool enable, FubiPredefinedGestures.Combinations combinationID = FubiPredefinedGestures.Combinations.NUM_COMBINATIONS)
        {
            FubiInternal.setAutoStartCombinationRecognition(enable, combinationID);
        }

        public static bool getAutoStartCombinationRecognition(FubiPredefinedGestures.Combinations combinationID = FubiPredefinedGestures.Combinations.NUM_COMBINATIONS)
        {
            return FubiInternal.getAutoStartCombinationRecognition(combinationID);
        }

        public static void enableCombinationRecognition(FubiPredefinedGestures.Combinations combinationID, UInt32 userID, bool enable)
        {
            FubiInternal.enableCombinationRecognition(combinationID, userID, enable);
        }

        public static FubiUtils.RecognitionResult getCombinationRecognitionProgressOn(FubiPredefinedGestures.Combinations combinationID, UInt32 userID, out FubiUtils.RecognitionCorrectionHint correctionHint, bool restart = true)
        {
            FubiUtils.RecognitionCorrectionHint hint = new FubiUtils.RecognitionCorrectionHint();
            FubiUtils.RecognitionResult res = FubiInternal.getCombinationRecognitionProgressOn(combinationID, userID, new IntPtr(0), restart, false, hint);
            correctionHint = hint;
            return res;
        }
        public static FubiUtils.RecognitionResult getCombinationRecognitionProgressOn(FubiPredefinedGestures.Combinations combinationID, UInt32 userID, bool restart = true)
        {
            return FubiInternal.getCombinationRecognitionProgressOn(combinationID, userID, new IntPtr(0), restart, false);
        }

        public static FubiUtils.RecognitionResult getCombinationRecognitionProgressOn(FubiPredefinedGestures.Combinations combinationID, UInt32 userID, out FubiTrackingData[] userStates,
            bool restart = true, bool returnFilteredData = false)
        {
            FubiUtils.RecognitionCorrectionHint dummy = new FubiUtils.RecognitionCorrectionHint();
            return getCombinationRecognitionProgressOn(combinationID, userID, out userStates, out dummy, restart, returnFilteredData);
        }
        public static FubiUtils.RecognitionResult getCombinationRecognitionProgressOn(FubiPredefinedGestures.Combinations combinationID, UInt32 userID, out FubiTrackingData[] userStates,
            out FubiUtils.RecognitionCorrectionHint correctionHint, bool restart = true, bool returnFilteredData = false)
        {
            IntPtr vec = FubiInternal.createTrackingDataVector();
            FubiUtils.RecognitionCorrectionHint hint = new FubiUtils.RecognitionCorrectionHint();
            FubiUtils.RecognitionResult recognized = FubiInternal.getCombinationRecognitionProgressOn(combinationID, userID, vec, restart, returnFilteredData, hint);
            correctionHint = hint;
            if (recognized == FubiUtils.RecognitionResult.RECOGNIZED)
            {
                UInt32 size = FubiInternal.getTrackingDataVectorSize(vec);
                userStates = new FubiTrackingData[size];
                for (UInt32 i = 0; i < size; i++)
                {
                    IntPtr tInfo = FubiInternal.getTrackingData(vec, i);
                    FubiTrackingData info = new FubiTrackingData();
                    for (UInt32 j = 0; j < (uint)FubiUtils.SkeletonJoint.NUM_JOINTS; ++j)
                    {
                        FubiInternal.getSkeletonJointPosition(tInfo, (FubiUtils.SkeletonJoint)j, out info.jointPositions[j].x, out info.jointPositions[j].y, out info.jointPositions[j].z, out info.jointPositions[j].confidence, out info.timeStamp);
                        double timeStamp = 0;
                        float[] rotMat = new float[9];
                        FubiInternal.getSkeletonJointOrientation(tInfo, (FubiUtils.SkeletonJoint)j, rotMat, out info.jointPositions[j].confidence, out timeStamp);
                        FubiUtils.Math.rotMatToRotation(rotMat, out info.jointOrientations[j].rx, out info.jointOrientations[j].ry, out info.jointOrientations[j].rz);
                        info.timeStamp = Math.Max(timeStamp, info.timeStamp);
                    }
                    userStates[i] = info;
                }
                FubiInternal.releaseTrackingDataVector(vec);
            }
            else
                userStates = null;
            return recognized;
        }

        public static bool loadRecognizersFromXML(string fileName)
        {
            IntPtr fileNamePtr = Marshal.StringToHGlobalAnsi(fileName);
            bool ret = FubiInternal.loadRecognizersFromXML(fileNamePtr);
            Marshal.FreeHGlobal(fileNamePtr);
            return ret;
        }

        public static FubiUtils.RecognitionResult recognizeGestureOn(string recognizerName, UInt32 userID)
        {
            IntPtr namePtr = Marshal.StringToHGlobalAnsi(recognizerName);
            FubiUtils.RecognitionResult ret = FubiInternal.recognizeGestureOn(namePtr, userID);
            Marshal.FreeHGlobal(namePtr);
            return ret;
        }
        public static FubiUtils.RecognitionResult recognizeGestureOn(string recognizerName, UInt32 userID, out FubiUtils.RecognitionCorrectionHint correctionHint)
        {
            IntPtr namePtr = Marshal.StringToHGlobalAnsi(recognizerName);
            FubiUtils.RecognitionCorrectionHint hint = new FubiUtils.RecognitionCorrectionHint();
            FubiUtils.RecognitionResult ret = FubiInternal.recognizeGestureOn(namePtr, userID, hint);
            correctionHint = hint;
            Marshal.FreeHGlobal(namePtr);
            return ret;
        }

        public static bool isInitialized()
        {
          return FubiInternal.isInitialized();
        }

        public static UInt32 getNumUserDefinedRecognizers()
        {
            return FubiInternal.getNumUserDefinedRecognizers();
        }

        public static void getDepthResolution(out Int32 width, out Int32 height)
        {
            FubiInternal.getDepthResolution(out width, out height);
        }

        public static void getRgbResolution(out Int32 width, out Int32 height)
        {
            FubiInternal.getRgbResolution(out width, out height);
        }

        public static void getIRResolution(out Int32 width, out Int32 height)
        {
            FubiInternal.getIRResolution(out width, out height);
        }

        public static string getUserDefinedRecognizerName(UInt32 recognizerIndex)
        {
            IntPtr namePtr = FubiInternal.getUserDefinedRecognizerName(recognizerIndex);
            return Marshal.PtrToStringAnsi(namePtr);
        }

        public static UInt32 getUserDefinedRecognizerIndex(string recognizerName)
        {
            IntPtr namePtr = Marshal.StringToHGlobalAnsi(recognizerName);
            UInt32 ret = FubiInternal.getUserDefinedRecognizerIndex(namePtr);
            Marshal.FreeHGlobal(namePtr);
            return ret;
        }

        public static Int32 getFingerCount(UInt32 userID, bool leftHand)
        {
            return FubiInternal.getFingerCount(userID, leftHand);
        }

        public static FubiUtils.RecognitionResult getCombinationRecognitionProgressOn(string recognizerName, UInt32 userID,
            bool restart = true)
        {
            FubiUtils.RecognitionCorrectionHint hint = new FubiUtils.RecognitionCorrectionHint();
            return getCombinationRecognitionProgressOn(recognizerName, userID, out hint, restart);
        }
        public static FubiUtils.RecognitionResult getCombinationRecognitionProgressOn(string recognizerName, UInt32 userID,
            out FubiUtils.RecognitionCorrectionHint correctionHint, bool restart = true)
        {
            IntPtr namePtr = Marshal.StringToHGlobalAnsi(recognizerName);
            FubiUtils.RecognitionCorrectionHint hint = new FubiUtils.RecognitionCorrectionHint();
            FubiUtils.RecognitionResult ret = FubiInternal.getCombinationRecognitionProgressOn(namePtr, userID, new IntPtr(0), restart, false, hint);
            correctionHint = hint;
            Marshal.FreeHGlobal(namePtr);
            return ret;
        }

        public static FubiUtils.RecognitionResult getCombinationRecognitionProgressOn(string recognizerName, UInt32 userID, out FubiTrackingData[] userStates,
           bool restart = true, bool returnFilteredData = false)
        {
            FubiUtils.RecognitionCorrectionHint hint = new FubiUtils.RecognitionCorrectionHint();
            return getCombinationRecognitionProgressOn(recognizerName, userID, out userStates, out hint, restart, returnFilteredData);
        }
        public static FubiUtils.RecognitionResult getCombinationRecognitionProgressOn(string recognizerName, UInt32 userID, out FubiTrackingData[] userStates,
            out FubiUtils.RecognitionCorrectionHint correctionHint, bool restart = true, bool returnFilteredData = false)
        {
            IntPtr vec = FubiInternal.createTrackingDataVector();
            IntPtr namePtr = Marshal.StringToHGlobalAnsi(recognizerName);
            FubiUtils.RecognitionCorrectionHint hint = new FubiUtils.RecognitionCorrectionHint();
            FubiUtils.RecognitionResult recognized = FubiInternal.getCombinationRecognitionProgressOn(namePtr, userID, vec, restart, returnFilteredData, hint);
            correctionHint = hint;
            Marshal.FreeHGlobal(namePtr);
            if (recognized == FubiUtils.RecognitionResult.RECOGNIZED)
            {
                UInt32 size = FubiInternal.getTrackingDataVectorSize(vec);
                userStates = new FubiTrackingData[size];
                for (UInt32 i = 0; i < size; i++)
                {
                    IntPtr tInfo = FubiInternal.getTrackingData(vec, i);
                    FubiTrackingData info = new FubiTrackingData();
                    for (UInt32 j = 0; j < (uint)FubiUtils.SkeletonJoint.NUM_JOINTS; ++j)
                    {
                        FubiInternal.getSkeletonJointPosition(tInfo, (FubiUtils.SkeletonJoint)j, out info.jointPositions[j].x, out info.jointPositions[j].y, out info.jointPositions[j].z, out info.jointPositions[j].confidence, out info.timeStamp);
                        double timeStamp = 0;
                        float[] rotMat = new float[9];
                        FubiInternal.getSkeletonJointOrientation(tInfo, (FubiUtils.SkeletonJoint)j, rotMat, out info.jointPositions[j].confidence, out timeStamp);
                        FubiUtils.Math.rotMatToRotation(rotMat, out info.jointOrientations[j].rx, out info.jointOrientations[j].ry, out info.jointOrientations[j].rz);
                        info.timeStamp = Math.Max(timeStamp, info.timeStamp);
                    }
                    userStates[i] = info;
                }
            }
            else
                userStates = null;

            FubiInternal.releaseTrackingDataVector(vec);
            return recognized;
        }

        public static Int32 getUserDefinedCombinationRecognizerIndex(string recognizerName)
        {
            IntPtr namePtr = Marshal.StringToHGlobalAnsi(recognizerName);
            Int32 ret = FubiInternal.getUserDefinedCombinationRecognizerIndex(namePtr);
            Marshal.FreeHGlobal(namePtr);
            return ret;
        }

        public static UInt32 getNumUserDefinedCombinationRecognizers()
        {
            return FubiInternal.getNumUserDefinedCombinationRecognizers();
        }

        public static string getUserDefinedCombinationRecognizerName(UInt32 recognizerIndex)
        {
            return Marshal.PtrToStringAnsi(FubiInternal.getUserDefinedCombinationRecognizerName(recognizerIndex));
        }

        public static void enableCombinationRecognition(string combinationName, UInt32 userID, bool enable)
        {
            IntPtr namePtr = Marshal.StringToHGlobalAnsi(combinationName);
            FubiInternal.enableCombinationRecognition(namePtr, userID, enable);
            Marshal.FreeHGlobal(namePtr);
        }

        public static void getColorForUserID(UInt32 id, out float r, out float g, out float b)
        {
            FubiInternal.getColorForUserID(id, out r, out g, out b);
        }


        public static bool saveImage(string fileName, int jpegQuality /*0-100*/,
            FubiUtils.ImageType type, FubiUtils.ImageNumChannels numChannels, FubiUtils.ImageDepth depth,
            int renderOptions = (int)FubiUtils.RenderOptions.Default,
            int jointsToRender = (int)FubiUtils.JointsToRender.ALL_JOINTS,
            FubiUtils.DepthImageModification depthModifications = FubiUtils.DepthImageModification.UseHistogram,
            UInt32 userId = 0, FubiUtils.SkeletonJoint jointOfInterest = FubiUtils.SkeletonJoint.NUM_JOINTS)
        {
            IntPtr namePtr = Marshal.StringToHGlobalAnsi(fileName);
            bool ret = FubiInternal.saveImage(namePtr, jpegQuality, type, numChannels, depth, renderOptions, jointsToRender, depthModifications, userId, jointOfInterest);
            Marshal.FreeHGlobal(namePtr);
            return ret;
        }

        public static UInt32 addJointRelationRecognizer(FubiUtils.SkeletonJoint joint, FubiUtils.SkeletonJoint relJoint,
            float minX = -FubiUtils.Math.MaxFloat, float minY = -FubiUtils.Math.MaxFloat, float minZ = -FubiUtils.Math.MaxFloat,
            float maxX = FubiUtils.Math.MaxFloat, float maxY = FubiUtils.Math.MaxFloat, float maxZ = FubiUtils.Math.MaxFloat,
            float minDistance = 0, float maxDistance = FubiUtils.Math.MaxFloat,
            bool useLocalPositions = false,
            Int32 atIndex = -1, string name = null,
            float minConfidene = -1.0f, FubiUtils.BodyMeasurement measuringUnit = FubiUtils.BodyMeasurement.NUM_MEASUREMENTS,
            bool useFilteredData = false)
        {
            IntPtr namePtr = Marshal.StringToHGlobalAnsi(name);
            UInt32 ret = FubiInternal.addJointRelationRecognizer(joint, relJoint, minX, minY, minZ, maxX, maxY, maxZ,
                minDistance, maxDistance, useLocalPositions, atIndex, namePtr, minConfidene, measuringUnit, useFilteredData);
            Marshal.FreeHGlobal(namePtr);
            return ret;
        }

        public static UInt32 addFingerCountRecognizer(FubiUtils.SkeletonJoint handJoint,
            UInt32 minFingers, UInt32 maxFingers,
            Int32 atIndex = -1,
            string name = null,
            float minConfidence = -1.0f,
            bool useMedianCalculation = false,
            bool useFilteredData = false)
        {
            IntPtr namePtr = Marshal.StringToHGlobalAnsi(name);
            UInt32 ret = FubiInternal.addFingerCountRecognizer(handJoint, minFingers, maxFingers,
                atIndex, namePtr, minConfidence, useMedianCalculation, useFilteredData);
            Marshal.FreeHGlobal(namePtr);
            return ret;
        }

        public static UInt32 addJointOrientationRecognizer(FubiUtils.SkeletonJoint joint,
            float minX = -180.0f, float minY = -180.0f, float minZ = -180.0f,
            float maxX = 180.0f, float maxY = 180.0f, float maxZ = 180.0f,
            bool useLocalOrientations = true,
            int atIndex = -1,
            string name = null,
            float minConfidence = -1,
            bool useFilteredData = false)
        {
            IntPtr namePtr = Marshal.StringToHGlobalAnsi(name);
            UInt32 ret = FubiInternal.addJointOrientationRecognizer(joint, minX, minY, minZ, maxX, maxY, maxZ,
                useLocalOrientations, atIndex, namePtr, minConfidence, useFilteredData);
            Marshal.FreeHGlobal(namePtr);
            return ret;
        }

        public static UInt32 addJointOrientationRecognizer(FubiUtils.SkeletonJoint joint,
            float orientX, float orientY, float orientZ,
            float maxAngleDiff = 45.0f,
            bool useLocalOrientations = true,
            int atIndex = -1,
            string name = null,
            float minConfidence = -1,
            bool useFilteredData = false)
        {
            IntPtr namePtr = Marshal.StringToHGlobalAnsi(name);
            UInt32 ret = FubiInternal.addJointOrientationRecognizer(joint, orientX, orientY, orientZ, maxAngleDiff,
                useLocalOrientations, atIndex, namePtr, minConfidence, useFilteredData);
            Marshal.FreeHGlobal(namePtr);
            return ret;
        }

        public static UInt32 addLinearMovementRecognizer(FubiUtils.SkeletonJoint joint,
            float dirX, float dirY, float dirZ, float minVel = 0, float maxVel = FubiUtils.Math.MaxFloat,
            bool useLocalPositions = false,
            int atIndex = -1, string name = null, float maxAngleDifference = 45.0f, bool useOnlyCorrectDirectionComponent = true,
            bool useFilteredData = false)
        {
            IntPtr namePtr = Marshal.StringToHGlobalAnsi(name);
            UInt32 ret = FubiInternal.addLinearMovementRecognizer(joint, dirX, dirY, dirZ, minVel, maxVel, 
                useLocalPositions, atIndex, namePtr, maxAngleDifference, useOnlyCorrectDirectionComponent, useFilteredData);
            Marshal.FreeHGlobal(namePtr);
            return ret;
        }
        public static UInt32 addLinearMovementRecognizer(FubiUtils.SkeletonJoint joint, FubiUtils.SkeletonJoint relJoint,
            float dirX, float dirY, float dirZ, float minVel = 0, float maxVel = FubiUtils.Math.MaxFloat,
            bool useLocalPositions = false,
            int atIndex = -1, string name = null, float maxAngleDifference = 45.0f, bool useOnlyCorrectDirectionComponent = true,
            bool useFilteredData = false)
        {
            IntPtr namePtr = Marshal.StringToHGlobalAnsi(name);
            UInt32 ret = FubiInternal.addLinearMovementRecognizer(joint, relJoint, dirX, dirY, dirZ, minVel, maxVel,
                useLocalPositions, atIndex, namePtr, maxAngleDifference, useOnlyCorrectDirectionComponent, useFilteredData);
            Marshal.FreeHGlobal(namePtr);
            return ret;
        }
        public static UInt32 addLinearMovementRecognizer(FubiUtils.SkeletonJoint joint,
            float dirX, float dirY, float dirZ, float minVel, float maxVel,
            float minLength, float maxLength, FubiUtils.BodyMeasurement measuringUnit,
            bool useLocalPositions = false,
            int atIndex = -1, string name = null, float maxAngleDifference = 45.0f, bool useOnlyCorrectDirectionComponent = true,
            bool useFilteredData = false)
        {
            IntPtr namePtr = Marshal.StringToHGlobalAnsi(name);
            UInt32 ret = FubiInternal.addLinearMovementRecognizer(joint, dirX, dirY, dirZ, minVel, maxVel,
                minLength, maxLength, measuringUnit, useLocalPositions, atIndex, namePtr, maxAngleDifference,
                useOnlyCorrectDirectionComponent, useFilteredData);
            Marshal.FreeHGlobal(namePtr);
            return ret;
        }
        public static UInt32 addLinearMovementRecognizer(FubiUtils.SkeletonJoint joint, FubiUtils.SkeletonJoint relJoint,
            float dirX, float dirY, float dirZ, float minVel, float maxVel,
            float minLength, float maxLength, FubiUtils.BodyMeasurement measuringUnit,
            bool useLocalPositions = false,
            int atIndex = -1, string name = null, float maxAngleDifference = 45.0f, bool useOnlyCorrectDirectionComponent = true,
            bool useFilteredData = false)
        {
            IntPtr namePtr = Marshal.StringToHGlobalAnsi(name);
            UInt32 ret = FubiInternal.addLinearMovementRecognizer(joint, relJoint, dirX, dirY, dirZ, minVel, maxVel,
                minLength, maxLength, measuringUnit, useLocalPositions, atIndex, namePtr, maxAngleDifference, 
                useOnlyCorrectDirectionComponent, useFilteredData);
            Marshal.FreeHGlobal(namePtr);
            return ret;
        }

        public static bool addCombinationRecognizer(string xmlDefinition)
        {
            IntPtr stringPtr = Marshal.StringToHGlobalAnsi(xmlDefinition);
            bool ret = FubiInternal.addCombinationRecognizer(stringPtr);
            Marshal.FreeHGlobal(stringPtr);
            return ret;
        }

        public static bool isUserInScene(UInt32 userID)
        {
            return FubiInternal.isUserInScene(userID);
        }

        public static bool isUserTracked(UInt32 userID)
        {
            return FubiInternal.isUserTracked(userID);
        }


        public static void getCurrentSkeletonJointPosition(UInt32 userID, FubiUtils.SkeletonJoint joint,
            out float x, out float y, out float z, out float confidence, out double timeStamp,
            bool useLocalPositions = false, bool useFilteredData = false)
        {
            IntPtr info = FubiInternal.getCurrentTrackingData(userID, useFilteredData);
            FubiInternal.getSkeletonJointPosition(info, joint, out x, out y, out z, out confidence, out timeStamp, useLocalPositions);
        }

        public static void getCurrentSkeletonJointOrientation(UInt32 userID, FubiUtils.SkeletonJoint joint,
            float[] mat, out float confidence, out double timeStamp,
            bool useFilteredData = false)
        {
            IntPtr info = FubiInternal.getCurrentTrackingData(userID, useFilteredData);
            if (info.ToInt32() != 0)
            {
                FubiInternal.getSkeletonJointOrientation(info, joint, mat, out confidence, out timeStamp);
            }
            else
            {
                mat = new float[] {1,0,0,0,1,0,0,0,1};
                confidence = 0;
                timeStamp = 0;
            }
        }

        public static void clearUserDefinedRecognizers()
        {
            FubiInternal.clearUserDefinedRecognizers();
        }

        public static UInt32 getClosestUserID()
        {
            return FubiInternal.getClosestUserID();
        }

        public static UInt16 getNumUsers()
        {
            return FubiInternal.getCurrentUsers(new IntPtr(0));
        }

        public static void realWorldToProjective(float realWorldX, float realWorldY, float realWorldZ, out float screenX, out float screenY, out float screenZ)
        {
            FubiInternal.realWorldToProjective(realWorldX, realWorldY, realWorldZ, out screenX, out screenY, out screenZ);
        }

        public static void resetTracking()
        {
            FubiInternal.resetTracking();
        }

        public static double getCurrentTime()
        {
            return FubiInternal.getCurrentTime();
        }

        public static void getFilterOptions(out float minCutOffFrequency, out float velocityCutOffFrequency, out float cutOffSlope)
        {
            FubiInternal.getFilterOptions(out minCutOffFrequency, out velocityCutOffFrequency, out cutOffSlope);
        }

        public static void setFilterOptions(float minCutOffFrequency = 1.0f, float velocityCutOffFrequency = 1.0f, float cutOffSlope = 0.007f)
        {
            FubiInternal.setFilterOptions(minCutOffFrequency, velocityCutOffFrequency, cutOffSlope);
        }

        public static uint getClosestUserIDs(uint[] ids, int maxUsers = -1)
        {
            return FubiInternal.getClosestUserIDs(ids, maxUsers);
        }

        public static void updateTrackingData(uint userId, FubiTrackingData data, FubiAccelerationData accelData = null)
        {
            GCHandle hSkeleton = new GCHandle();
            GCHandle hAccel = new GCHandle();
            IntPtr skelPointer = new IntPtr(0);
            IntPtr accelPointer = new IntPtr(0);
            double timeStamp = -1;

            if (data != null)
            {
                float[] skeletonArray = data.getArray();
                hSkeleton = GCHandle.Alloc(skeletonArray, GCHandleType.Pinned);
                skelPointer = hSkeleton.AddrOfPinnedObject();
                timeStamp = data.timeStamp;
            }
            if (accelData != null)
            {
                float[] accelArray = accelData.getArray();
                hAccel = GCHandle.Alloc(accelArray, GCHandleType.Pinned);
                accelPointer = hAccel.AddrOfPinnedObject();
                if (timeStamp < 0)
                    timeStamp = accelData.timeStamp;
            }

            FubiInternal.updateTrackingData(userId, skelPointer, timeStamp, accelPointer);

            if (hSkeleton.IsAllocated)
                hSkeleton.Free();
            if (hAccel.IsAllocated)
                hAccel.Free();
        }

        public static UInt32 addLinearAccelerationRecognizer( FubiUtils.SkeletonJoint joint,
		    float dirX, float dirY, float dirZ, float minAccel, float maxAccel = FubiUtils.Math.MaxFloat,
		    int atIndex = -1,
		    string name = null,
		    float maxAngleDifference = 45.0f,
            bool useOnlyCorrectDirectionComponent= true)
        {
            IntPtr namePtr = Marshal.StringToHGlobalAnsi(name);
            UInt32 ret = FubiInternal.addLinearAccelerationRecognizer(joint, dirX, dirY, dirZ, minAccel, maxAccel,
                atIndex, namePtr, maxAngleDifference, useOnlyCorrectDirectionComponent);
            Marshal.FreeHGlobal(namePtr);
            return ret;
        }

        public static uint addAngularMovementRecognizer(FubiUtils.SkeletonJoint joint,
            float minVelX = -FubiUtils.Math.MaxFloat, float minVelY = -FubiUtils.Math.MaxFloat, float minVelZ = -FubiUtils.Math.MaxFloat,
		    float maxVelX = FubiUtils.Math.MaxFloat, float maxVelY = FubiUtils.Math.MaxFloat, float maxVelZ =  FubiUtils.Math.MaxFloat,
		    bool useLocalOrients = true,
            int atIndex = -1, string name = null,
		    float minConfidence = -1.0f, bool useFilteredData = false)
        {
            IntPtr namePtr = Marshal.StringToHGlobalAnsi(name);
            UInt32 ret = FubiInternal.addAngularMovementRecognizer(joint, minVelX, minVelY, minVelZ,
                maxVelX, maxVelY, maxVelZ, useLocalOrients,
                atIndex, namePtr, minConfidence, useFilteredData);
            Marshal.FreeHGlobal(namePtr);
            return ret;
        }

        public static int getCurrentCombinationRecognitionState(string recognizerName, uint userID, out uint numStates, out bool isInterrupted, out bool isInTransition)
        {
            IntPtr namePtr = Marshal.StringToHGlobalAnsi(recognizerName);
            int ret = FubiInternal.getCurrentCombinationRecognitionState(namePtr, userID, out numStates, out isInterrupted, out isInTransition);
            Marshal.FreeHGlobal(namePtr);
            return ret;
        }

        public static bool initFingerSensor(FubiUtils.FingerSensorType type, float offsetPosX = 0, float offsetPosY = -600.0f, float offsetPosZ = 200.0f)
        {
            return FubiInternal.initFingerSensor(type, offsetPosX, offsetPosY, offsetPosZ);
        }

        public static int getAvailableFingerSensorTypes()
        {
            return FubiInternal.getAvailableFingerSensorTypes();
        }

        public static FubiUtils.FingerSensorType getCurrentFingerSensorType()
        {
            return FubiInternal.getCurrentFingerSensorType();
        }

        public static ushort getNumHands()
        {
            return FubiInternal.getNumHands();
        }

        public static uint getHandID(uint index)
        {
            return FubiInternal.getHandID(index);
        }

        public static FubiUtils.RecognitionResult recognizeGestureOnHand(string recognizerName, uint handID)
        {
            IntPtr namePtr = Marshal.StringToHGlobalAnsi(recognizerName);
            FubiUtils.RecognitionResult ret = FubiInternal.recognizeGestureOnHand(namePtr, handID);
            Marshal.FreeHGlobal(namePtr);
            return ret;
        }

        public static FubiUtils.RecognitionResult getCombinationRecognitionProgressOnHand(string recognizerName, uint handID, bool restart = true)
        {
            FubiUtils.RecognitionCorrectionHint hint = new FubiUtils.RecognitionCorrectionHint();
            return getCombinationRecognitionProgressOnHand(recognizerName, handID, out hint, restart);
        }

        public static FubiUtils.RecognitionResult getCombinationRecognitionProgressOnHand(string recognizerName, uint handID, out FubiUtils.RecognitionCorrectionHint correctionHint, bool restart = true)
        {          
            IntPtr namePtr = Marshal.StringToHGlobalAnsi(recognizerName);
            FubiUtils.RecognitionCorrectionHint hint = new FubiUtils.RecognitionCorrectionHint();
            FubiUtils.RecognitionResult ret = FubiInternal.getCombinationRecognitionProgressOnHand(namePtr, handID, new IntPtr(0), restart, false, hint);
            correctionHint = hint;
            Marshal.FreeHGlobal(namePtr);
            return ret;
        }

        public static void enableCombinationRecognitionHand(string combinationName, uint handID, bool enable)
        {
            IntPtr namePtr = Marshal.StringToHGlobalAnsi(combinationName);
            FubiInternal.enableCombinationRecognitionHand(namePtr, handID, enable);
            Marshal.FreeHGlobal(namePtr);
        }

        public static int getCurrentCombinationRecognitionStateForHand(string recognizerName, uint handID, out uint numStates, out bool isInterrupted, out bool isInTransition)
        {
            IntPtr namePtr = Marshal.StringToHGlobalAnsi(recognizerName);
            int ret = FubiInternal.getCurrentCombinationRecognitionStateForHand(namePtr, handID, out numStates, out isInterrupted, out isInTransition);
            Marshal.FreeHGlobal(namePtr);
            return ret;
        }

        /**
	     * \brief Get the offset position of the current finger sensor to the main sensor
	     * 
	     * @param xOffset, yOffset, zOffset [out] a vector from the main sensor to the finger sensor, (0,0,0) if no sensor present
	     */
        public static void getFingerSensorOffsetPosition(out float xOffset, out float yOffset, out float zOffset)
        {
            FubiInternal.getFingerSensorOffsetPosition(out xOffset, out yOffset, out zOffset);
        }

        /**
         * \brief Set the offset position of the current finger sensor to the main sensor
         * 
         * @param xOffset, yOffset, zOffset the vector from the main sensor to the finger sensor
         */
        public static void setFingerSensorOffsetPosition(float xOffset, float yOffset, float zOffset)
        {
            FubiInternal.setFingerSensorOffsetPosition(xOffset, yOffset, zOffset);
        }

        /**
	     * \brief Get the target sensor of a user defined combination recognizer
	     * 
	     * @param recognizerName name of the combination
	     * @return the target sensor as defined in FubiUtils.h
	     */
        public static FubiUtils.RecognizerTarget getCombinationRecognitizerTargetSensor(string recognizerName)
        {
            IntPtr namePtr = Marshal.StringToHGlobalAnsi(recognizerName);
            FubiUtils.RecognizerTarget ret = FubiInternal.getCombinationRecognizerTargetSensor(namePtr);
            Marshal.FreeHGlobal(namePtr);
            return ret;
        }

        /**
         * \brief Get the target sensor for a recognizer
         * 
         * @param recognizerName name of the recognizer
         * @return the target sensor as defined in FubiUtils.h
         */
        public static FubiUtils.RecognizerTarget getRecognizerTargetSensor(string recognizerName)
        {
            IntPtr namePtr = Marshal.StringToHGlobalAnsi(recognizerName);
            FubiUtils.RecognizerTarget ret = FubiInternal.getRecognizerTargetSensor(namePtr);
            Marshal.FreeHGlobal(namePtr);
            return ret;
        }

        /**
         * \brief Get meta information of a state of one recognizers
         * 
         * @param recognizerName name of the combination
         * @param stateIndex the state index to get the meta info from
         * @param propertyName the name of the property to get
         * @return the value of the requested meta info property as a string, or 0x0 on error
         */
        public static string getCombinationRecognitionStateMetaInfo(string recognizerName, uint stateIndex, string propertyName)
        {
            IntPtr namePtr = Marshal.StringToHGlobalAnsi(recognizerName);
            IntPtr propertyPtr = Marshal.StringToHGlobalAnsi(propertyName);
            string info = Marshal.PtrToStringAnsi(FubiInternal.getCombinationRecognitionStateMetaInfo(namePtr, stateIndex, propertyPtr));
            Marshal.FreeHGlobal(namePtr);
            Marshal.FreeHGlobal(propertyPtr);
            return info;
        }

        /*! @}*/
    }
}

