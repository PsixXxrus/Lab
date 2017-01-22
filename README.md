#include <string>
#include <iostream>
#include <fstream>
#include <locale>
#include <fstream>
#include <cstdlib>
#include <stdlib.h>
#include <string.h>
int const MaxSotr = 2;
int SystolicPressure;
int DiastolicPressure;
using namespace std;
struct DdistrictHospital
{
	char *FamilyName[32];
	char NameUser[32];
	char MiddleName[32];
	/*string Address;
	string HeightUser;
	string WeightUser;
	string PulseUser;*/
};
void FirstDegreeHypertension(DdistrictHospital sotr[], ofstream &ofs)
{
	for (int i = 0; i < MaxSotr; i++)
	{
		if (((SystolicPressure >= 160) && (SystolicPressure <= 179)) || ((DiastolicPressure >= 100) && (DiastolicPressure <= 114)))
		{
			ofs << sotr[i].FamilyName << ' ' << sotr[i].NameUser << ' ' << sotr[i].MiddleName << ' ' << "- гипотермик [1 Степень]" << endl;
			cout << sotr[i].FamilyName << ' ' << sotr[i].NameUser << ' ' << sotr[i].MiddleName << ' ' << "- гипотермик [1 Степень]" << endl;
		}
	}
}
void SecondDegreeHypertension(DdistrictHospital sotr[], ofstream &ofs)
{
	for (int i = 0; i < MaxSotr; i++)
	{
		if (((SystolicPressure >= 180) && (SystolicPressure <= 234)) || ((DiastolicPressure >= 115) && (DiastolicPressure <= 129)))
		{
			ofs << sotr[i].FamilyName << ' ' << sotr[i].NameUser << ' ' << sotr[i].MiddleName << ' ' << "- гипотермик [2 Степень]" << endl;
			cout << sotr[i].FamilyName << ' ' << sotr[i].NameUser << ' ' << sotr[i].MiddleName << ' ' << "- гипотермик [2 Степень]" << endl;
		}
	}
}
void ThirdDegreeHypertension(DdistrictHospital sotr[], ofstream &ofs)
{
	for (int i = 0; i < MaxSotr; i++)
	{
		if ((SystolicPressure >= 230) || (DiastolicPressure >= 130))
		{
			ofs << sotr[i].FamilyName << ' ' << sotr[i].NameUser << ' ' << sotr[i].MiddleName << ' ' << "- гипотермик [3 Степень]" << endl;
			cout << sotr[i].FamilyName << ' ' << sotr[i].NameUser << ' ' << sotr[i].MiddleName << ' ' << "- гипотермик [3 Степень]" << endl;
		}
	}
}
void AllDegreeHypertension(DdistrictHospital sotr[], ofstream &ofs)
{
	for (int i = 0; i < MaxSotr; i++)
	{
		if ((SystolicPressure >= 160) || (DiastolicPressure >= 100))
		{
			ofs << sotr[i].FamilyName << ' ' << sotr[i].NameUser << ' ' << sotr[i].MiddleName << ' ' << "- гипотермик" << endl;
			cout << sotr[i].FamilyName << ' ' << sotr[i].NameUser << ' ' << sotr[i].MiddleName << ' ' << "- гипотермик" << endl;
		}
	}
}
int comp(const void* a, const void* b);
int main() {
	setlocale(LC_ALL, "Russian");
	DdistrictHospital sotr[10];
	for (int i = 0; i < MaxSotr; i++)
	{
		cout << "Введите [Фамилию/Имя/Отчество]:" << endl;
		cout << "Фамилию - "; cin >> sotr[i].FamilyName;
		cout << "Имя - "; cin >> sotr[i].NameUser;
		cout << "Отчество - "; cin >> sotr[i].MiddleName;
		/*cout << "Введите ваш адрес:";
		cin >> sotr[i].Address;
		cout << "Введите [Рост/Вес/Пульс]:" << endl;
		cout << "Рост - "; cin >> sotr[i].HeightUser;
		cout << "Вес - "; cin >> sotr[i].WeightUser;
		cout << "Пульс - "; cin >> sotr[i].PulseUser; */
		cout << "Введите [Систолическое давление(SYS)]/[Диастолическое давление(DIA)]:" << endl;
		cout << "SYS - "; cin >> SystolicPressure;
		cout << "DIA - "; cin >> DiastolicPressure; cout << endl << endl;
	}
	qsort(sotr, 10, sizeof(sotr), comp);
	/*for (int i = 0; i<MaxSotr; i++)
	{
		for (int j = 0; j<MaxSotr; j++)
		{
			if (sotr[i].FamilyName[0] < sotr[j].FamilyName[0])
			{
				swap(sotr[i], sotr[j]);
			}
			else if (sotr[i].FamilyName[0] == sotr[j].FamilyName[0])
			{
				if (sotr[i].FamilyName[1] < sotr[j].FamilyName[1])
				{
					swap(sotr[i], sotr[j]);
				}
			}
		}
	}*/
	int DataProcessing = 0;
	cout << "1. - Первая степень гипертонии\n2. - Вторая степень гипертонии\n3. - Третья степень гипертонии\n4. - Показать всех гипертоников\n";
	cout << "Сделайте свой выбор...: " << endl;
	cin >> DataProcessing;
	switch (DataProcessing)
	{
	case 1:
	{
		ofstream ofs("FirstDegreeHypertension.txt");
		FirstDegreeHypertension(sotr, ofs);
		ofs.close();
	}
	break;
	case 2:
	{
		ofstream ofs("SecondDegreeHypertension.txt");
		SecondDegreeHypertension(sotr, ofs);
		ofs.close();
		break;
	}
	case 3:
	{
		ofstream ofs("ThirdDegreeHypertension.txt");
		ThirdDegreeHypertension(sotr, ofs);
		ofs.close();
		break;
	}
	case 4:
	{
		ofstream ofs("AllDegreeHypertension.txt");
		AllDegreeHypertension(sotr, ofs);
		ofs.close();
	}
	break;
	default:
		cout << "Ошибка. Вы ввели неправильное значение.\n";
		break;
	}
	system("pause");
	return 0;
}
int comp(const void * a, const void * b)
{
	return strcmp(*(char**)a, *(char**)b);
}
