#include "stdafx.h"
#include "LinkedList.h"


LinkedList::LinkedList(int val)
{
	m_val = val;
	m_pNext = NULL;
}

LinkedList::LinkedList(int val, LinkedList* pNext)
{
	m_val = val;
	m_pNext = pNext;
}

LinkedList::~LinkedList()
{
}

LinkedList* LinkedList::GetNext()
{
	return m_pNext;
}

void LinkedList::SetNext(LinkedList* pNode)
{
	m_pNext = pNode;
}

int LinkedList::GetVal()
{
	return m_val;
}

void LinkedList::AddHead(LinkedList** pHead, LinkedList* pNode)
{
	pNode->SetNext(*pHead);
	*pHead = pNode;
}