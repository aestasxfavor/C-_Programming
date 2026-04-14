#include <iostream>
using namespace std;

int main()
{

#pragma region Coiding Test 2026_04_05
	/*
	백준 18108번: 1998년생인 내가 태국에서는 2541년생?!

	int a;
	int b;

	cin >> a;
	b = a - 543;

	cout << b << endl;

	*/

	/*
	백준 10430번: 나머지

	int a;
	int b;
	int c;

	cin >> a;
	cin >> b;
	cin >> c;

	cout << (a + b) % c << endl;
	cout << ((a % c) + (b % c)) % c << endl;
	cout << (a * b) % c << endl;
	cout << ((a % c) * (b % c)) % c << endl;

*/

#pragma endregion

#pragma region Coding Test 2026_04_06
/*
* 백준 2588번: 곱셈
int a;
int b;

cin >> a;
cin >> b;

int c = b % 10;
int d = (b / 10) % 10;
int e = b / 100;

cout << a * c << endl;
cout << a * d << endl;
cout << a * e << endl;
cout << a * b << endl;
*/
#pragma endregion

#pragma region Coding Test 2026_04_12
/*
백준: 11382번 꼬마 정민

long long a, b, c, sum;

cin >> a >> b >> c;

sum = a + b + c;

cout << sum << endl;

*/
#pragma endregion

#pragma region Coding Test 2026_04_13
/*
백준: 10172번 고양이
cout << "\\    /\\" << endl;
cout << " )  ( ')" << endl;
cout << "(  /  )" << endl;
cout << " \\(__)|" << endl;

*/

/*
백준 10172번 강아지
cout << "|\\_/|" << endl;
cout << "|q p|   /}" << endl;
cout << "( 0 )\"\"\"\\" << endl;
cout << "|\"^\"`    |" << endl;
cout << "||_/=\\\\__|" << endl;

*/
#pragma endregion

#pragma region Coding Test 2026-04-14
/*
백준 1330번 두 수 비교하기

int a, b;
cin >> a >> b;

if (a > b)
{
	cout << ">";
}
else if (a < b)
{
	cout << "<";
}
else
{
	cout << "==";
}
*/

/* 
백준 9498 시험 성적
	int a;
	cin >> a;

	if (a >= 90)
	{
		cout << "A";
	}
	else if (a >= 80)
	{
		cout << "B";
	}
	else if (a >= 70)
	{
		cout << "C";
	}
	else if (a >= 60)
	{
		cout << "D";
	}
	else
	{
		cout << "F";
	}
*/

/*
백준 2753번 윤년

	int a;
	cin >> a;

	if (a % 4 == 0 && a % 100 != 0 || a % 400 == 0)
	{
		cout << "1";
	}
	else
	{
		cout << "0";
	}
*/
#pragma endregion







	

}
