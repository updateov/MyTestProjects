// InverseList.cpp : Defines the entry point for the console application.
//

#include "stdafx.h"
#include <iostream>
#include "LinkedList.h"
#include "InverseList.h"

using namespace std;

int main()
{
	LinkedList* pList = new LinkedList(0);
	for (int i = 1; i < 12; i++)
	{
		LinkedList* pNode = new LinkedList(i, pList);
		pList = pNode;
	}

	Empty* pP = new Empty;

	int s = sizeof(pP);
	int sc = sizeof(Empty);

	Inverse(&pList);



	LinkedList* p = pList;
	while (p != NULL)
	{
		cout << p->GetVal() << "  ";
		p = p->GetNext();
	}

	int n;
	cin >> n;
    return 0;
}

void Inverse(LinkedList** pHead)
{
	LinkedList* pNewHead = InverseRec(NULL, *pHead);
	*pHead = pNewHead;
}

LinkedList* InverseRec(LinkedList* pPrev, LinkedList* pCur)
{
	if (pCur->GetNext() == NULL)
	{
		pCur->SetNext(pPrev);
		return pCur;
	}

	LinkedList* pNext = pCur->GetNext();
	pCur->SetNext(pPrev);
	return InverseRec(pCur, pNext);
}
