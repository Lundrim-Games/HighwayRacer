﻿//----------------------------------------------
//           	   Highway Racer
//
// Copyright © 2014 - 2024 BoneCracker Games
// http://www.bonecrackergames.com
//
//----------------------------------------------

using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;

[CustomEditor(typeof(HR_PlayerCars))]
public class HR_PlayerCarsEditor : Editor {

    HR_PlayerCars prop;

    Vector2 scrollPos;
    List<HR_PlayerCars.Cars> playerCars = new List<HR_PlayerCars.Cars>();

    GUISkin skin;
    Color orgColor;

    private void OnEnable() {

        skin = Resources.Load<GUISkin>("HR_Gui");
        orgColor = GUI.color;

    }

    public override void OnInspectorGUI() {

        prop = (HR_PlayerCars)target;
        serializedObject.Update();
        GUI.skin = skin;

        orgColor = GUI.color;

        if (HR_PlayerCars.Instance.lastAdd != null)
            AddLatestCar();

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Player Cars Editor", EditorStyles.boldLabel);
        EditorGUILayout.LabelField("This editor will keep update necessary .asset files in your project. Don't change directory of the ''Resources/HR_Assets''.", EditorStyles.helpBox);
        EditorGUILayout.Space();

        scrollPos = EditorGUILayout.BeginScrollView(scrollPos, false, false);

        EditorGUIUtility.labelWidth = 120f;

        GUILayout.Label("Player Cars", EditorStyles.boldLabel);

        EditorGUI.indentLevel++;

        for (int i = 0; i < prop.cars.Length; i++) {

            EditorGUILayout.BeginVertical(GUI.skin.box);
            EditorGUILayout.Space();

            if (prop.cars[i].playerCar)
                EditorGUILayout.LabelField(prop.cars[i].playerCar.name, EditorStyles.boldLabel);

            prop.cars[i].vehicleName = EditorGUILayout.TextField("Player Car Name", prop.cars[i].vehicleName, GUILayout.MaxWidth(475f));

            EditorGUILayout.Space();

            prop.cars[i].playerCar = (GameObject)EditorGUILayout.ObjectField("Player Car Prefab", prop.cars[i].playerCar, typeof(GameObject), false, GUILayout.MaxWidth(475f));

            EditorGUILayout.HelpBox("You can edit vehicles in isolated prefab view by clicking the 'Edit RCCP' button.", MessageType.Info);

            if (GUILayout.Button("Edit RCCP"))
                Selection.activeGameObject = prop.cars[i].playerCar.gameObject;

            if (prop.cars[i].playerCar && prop.cars[i].playerCar.GetComponent<RCCP_CarController>()) {

                if (prop.cars[i].playerCar.GetComponent<HR_Player>() == null)
                    prop.cars[i].playerCar.AddComponent<HR_Player>();

                EditorGUILayout.Space();

                EditorUtility.SetDirty(prop.cars[i].playerCar);

            } else {

                EditorGUILayout.HelpBox("Select A RCC Based Car", MessageType.Error);

            }

            EditorGUILayout.Space();

            if (prop.cars[i].price <= 0)
                prop.cars[i].unlocked = true;

            if (prop.cars[i] != null && prop.cars[i].playerCar) {

                EditorGUILayout.BeginHorizontal();
                prop.cars[i].price = EditorGUILayout.IntField("Price", prop.cars[i].price, GUILayout.MaxWidth(200f));
                prop.cars[i].unlocked = EditorGUILayout.ToggleLeft("Unlocked", prop.cars[i].unlocked, GUILayout.MaxWidth(122f));
                EditorGUILayout.EndHorizontal();

            }

            EditorGUILayout.BeginHorizontal();

            GUILayout.FlexibleSpace();

            if (GUILayout.Button("\u2191", GUILayout.MaxWidth(25f)))
                Up(i);

            if (GUILayout.Button("\u2193", GUILayout.MaxWidth(25f)))
                Down(i);

            GUI.color = Color.red;

            if (GUILayout.Button("X", GUILayout.MaxWidth(25f)))
                RemoveCar(i);

            GUI.color = orgColor;

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();
            EditorGUILayout.EndVertical();

        }

        GUI.color = Color.cyan;

        if (GUILayout.Button("Create Player Car"))
            AddNewCar();

        if (GUILayout.Button("--< Return To General Settings"))
            OpenGeneralSettings();

        GUI.color = orgColor;

        EditorGUILayout.EndScrollView();

        EditorGUILayout.Space();

        EditorGUILayout.LabelField("Highway Racer 2" + HR_Version.version + "\nDeveloped by Ekrem Bugra Ozdoganlar\nBoneCrackerGames", EditorStyles.centeredGreyMiniLabel, GUILayout.MaxHeight(50f));

        if (GUI.changed)
            EditorUtility.SetDirty(prop);

        serializedObject.ApplyModifiedProperties();

    }

    private void AddLatestCar() {

        if (prop.lastAdd.playerCar == null)
            return;

        playerCars.Clear();
        playerCars.AddRange(prop.cars);
        playerCars.Add(HR_PlayerCars.Instance.lastAdd);
        prop.cars = playerCars.ToArray();
        PlayerPrefs.SetInt("SelectedPlayerCarIndex", 0);
        HR_PlayerCars.Instance.lastAdd = null;
        EditorUtility.SetDirty(prop);

    }

    private void AddNewCar() {

        playerCars.Clear();
        playerCars.AddRange(prop.cars);
        HR_PlayerCars.Cars newCar = new HR_PlayerCars.Cars();
        playerCars.Add(newCar);
        prop.cars = playerCars.ToArray();
        PlayerPrefs.SetInt("SelectedPlayerCarIndex", 0);

        EditorUtility.SetDirty(prop);

    }

    private void RemoveCar(int index) {

        playerCars.Clear();
        playerCars.AddRange(prop.cars);
        playerCars.RemoveAt(index);
        prop.cars = playerCars.ToArray();
        PlayerPrefs.SetInt("SelectedPlayerCarIndex", 0);

        EditorUtility.SetDirty(prop);

    }

    private void Up(int index) {

        if (index <= 0)
            return;

        playerCars.Clear();
        playerCars.AddRange(prop.cars);

        HR_PlayerCars.Cars currentCar = playerCars[index];
        HR_PlayerCars.Cars previousCar = playerCars[index - 1];

        playerCars.RemoveAt(index);
        playerCars.RemoveAt(index - 1);

        playerCars.Insert(index - 1, currentCar);
        playerCars.Insert(index, previousCar);

        prop.cars = playerCars.ToArray();
        PlayerPrefs.SetInt("SelectedPlayerCarIndex", 0);

        EditorUtility.SetDirty(prop);

    }

    private void Down(int index) {

        if (index >= prop.cars.Length - 1)
            return;

        playerCars.Clear();
        playerCars.AddRange(prop.cars);

        HR_PlayerCars.Cars currentCar = playerCars[index];
        HR_PlayerCars.Cars nextCar = playerCars[index + 1];

        playerCars.RemoveAt(index);
        playerCars.Insert(index, nextCar);

        playerCars.RemoveAt(index + 1);
        playerCars.Insert(index + 1, currentCar);

        prop.cars = playerCars.ToArray();
        PlayerPrefs.SetInt("SelectedPlayerCarIndex", 0);

        EditorUtility.SetDirty(prop);

    }

    private void OpenGeneralSettings() {

        Selection.activeObject = HR_Settings.Instance;

    }

}
