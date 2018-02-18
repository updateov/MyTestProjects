#include "stdafx.h"
#include "RBTree.h"

RBTree::RBTree()
{
	m_pElement = new RBTreeElement; 
	m_color = 0;
	m_pLeft = NULL;
	m_pParent = NULL;
	m_pRight = NULL;
}

RBTree::~RBTree()
{
}


RBTree* RBTree::GetDummy()
{
	return NULL;
}

RBTree* RBTree::SearchItem (RBTree* root, int val)   // Recursive
{
	RBTree* cur;
	cur = root;
	int curVal = cur->m_pElement->m_val;
	if (curVal == val)
		return (cur);
	else
		if (curVal < val)
			if (cur->m_pRight != NULL)
				return(SearchItem(cur->m_pRight, val));
			else
				return (NULL);
		else
			if (cur->m_pLeft != NULL)
				return(SearchItem(cur->m_pLeft, val));
			else
				return (NULL);
}


void RBTree::LeftRotate(RBTree **root, RBTree *vert)  // Iterative
{
	RBTree* cur;
	cur = vert->m_pRight;
	vert->m_pRight = cur->m_pLeft;
	if (cur->m_pLeft != NULL)
		cur->m_pLeft->m_pParent = vert;

	cur->m_pParent = vert->m_pParent;
	if (vert->m_pParent == NULL)
		*root = cur;
	else
		if (vert == vert->m_pParent->m_pLeft)
			vert->m_pParent->m_pLeft = cur;
		else
			vert->m_pParent->m_pRight = cur;

	cur->m_pLeft = vert;
	vert->m_pParent = cur;
}


void RBTree::RightRotate(RBTree **root, RBTree *vert)   // Iterative
{
	RBTree* cur;
	cur = vert->m_pLeft;
	vert->m_pLeft = cur->m_pRight;
	if (cur->m_pRight != NULL)
		cur->m_pRight->m_pParent = vert;

	cur->m_pParent = vert->m_pParent;
	if (vert->m_pParent == NULL)
		*root = cur;
	else
		if (vert == vert->m_pParent->m_pRight)
			vert->m_pParent->m_pRight = cur;
		else
			vert->m_pParent->m_pLeft = cur;

	cur->m_pRight = vert;
	vert->m_pParent = cur;
}

/*
 *	insert element to RB-BST needs four functions:
 *	tree_insert - adds an element to regular BST
 *	rb_insert - changes some colors and makes some rotations to restore
 *				the RB properties
 *	left_rotate - changes some pointers and it looks like CCW move
 *	right_rotate - changes some pointers and it looks like CW move
 */

/// tree insert
void RBTree::RBTreeInsert(RBTree **root, RBTree *box)   // Iterative
{
	RBTree* cur;
	box->m_pLeft = NULL;
	box->m_pRight = NULL;
	if ((!(*root)) || ((*root) == NULL))
	{
		*root = box;
		(*root)->m_pParent = NULL;
	}
	else {
		cur = *root;
		for ( ; ; )
		{
			int a = cur->m_pElement->m_val, b = box->m_pElement->m_val;
			if(a > b)
				if (cur->m_pLeft == NULL)
				{
					cur->m_pLeft = box;
					box->m_pParent = cur;
					return;
				}
				else
					cur = cur->m_pLeft;
			else
				if (cur->m_pRight == NULL)
				{
					cur->m_pRight = box;
					box->m_pParent = cur;
					return;
				}
				else
					cur = cur->m_pRight;
		}
	}
}


///RB Insert
void RBTree::RBInsert(RBTree **root, RBTree *box)   // Iterative
{
	RBTree* cur;
	RBTreeInsert (root, box);
	box->m_color = 1;
	while ((box != *root) && (box->m_pParent->m_color == 1)) 
	{ 
		if (box->m_pParent == box->m_pParent->m_pParent->m_pLeft) 
		{      
			cur = box->m_pParent->m_pParent->m_pRight;
			if (cur->m_color == 1) 
			{						  
				box->m_pParent->m_color = 0;
				cur->m_color = 0;
				box->m_pParent->m_pParent->m_color = 1;
				box = box->m_pParent->m_pParent;
			}												
			else 
			{											
				if (box == box->m_pParent->m_pRight) 
				{			
					box = box->m_pParent;
					LeftRotate (root, box);
				}											
				
				box->m_pParent->m_color = 0;
				box->m_pParent->m_pParent->m_color = 1;
				cur = box->m_pParent->m_pParent;
				RightRotate(root, cur);
			}												
		}													
		else 
		{												
			cur = box->m_pParent->m_pParent->m_pLeft;
			if (cur->m_color == 1) 
			{						
				box->m_pParent->m_color = 0;
				cur->m_color = 0;
				box->m_pParent->m_pParent->m_color = 1;
				box = box->m_pParent->m_pParent;
			}												
			else 
			{											
				if (box == box->m_pParent->m_pLeft)
				{			
					box = box->m_pParent;
					RightRotate(root, box);
				}
				
				box->m_pParent->m_color = 0;
				box->m_pParent->m_pParent->m_color = 1;
				cur = box->m_pParent->m_pParent;
				LeftRotate(root, cur);
			}
		}
	}
	
	(*root)->m_color = 0;
}


void RBTree::AddRBTree(RBTree **root, RBTreeElement* data)
{
	RBTree* box;
	box = new RBTree;
	box->m_pElement = data;
	RBInsert (root, box);
}

/*
 *	To delete box from RB-BST six functions are needed:
 *  rb_delete - is almost the same as tree_delete function
 *				used in regular BST's.
 *	rb_delete_fixup - makes some rotations and color changes
 *				to restore the RB properties.
 *	successor - returns the pointer to the next element greater than current
 *	minimum - returns pointer to the smallest element.
 *	left_rotate - explained earlier
 *	right_rotate - explained earlier
 */
RBTree* RBTree::RBDelete(RBTree **root, RBTree *vert)   // Iterative
{
	RBTree* toMov;
	RBTree* succ;
	if ((vert->m_pLeft == NULL) || (vert->m_pRight == NULL))
		succ = vert;
	else
		succ = Successor(vert);
	if (succ->m_pLeft != NULL)
		toMov = succ->m_pLeft;
	else
		toMov = succ->m_pRight;
	toMov->m_pParent = succ->m_pParent;
	if (succ->m_pParent == NULL)
		*root = toMov;
	else
		if (succ == succ->m_pParent->m_pLeft)
			succ->m_pParent->m_pLeft = toMov;
		else
			succ->m_pParent->m_pRight = toMov;
	if (succ != vert)
		vert->m_pElement = succ->m_pElement;
	if (succ->m_color == 0)
		RBDeleteFixup(root, toMov);
	return (succ);
}


void RBTree::RBDeleteFixup(RBTree **root, RBTree *vert)    // Iterative
{
	RBTree* cur;
	while ((vert != *root) && (vert->m_color == 0)) 
	{
		if (vert == vert->m_pParent->m_pLeft)
		{
			cur = vert->m_pParent->m_pRight;
			if (cur->m_color == 1)
			{
				cur->m_color = 0;
				vert->m_pParent->m_color = 1;
				LeftRotate(root, vert->m_pParent);
				cur = vert->m_pParent->m_pRight;
			}

			if ((cur->m_pLeft->m_color == 0) && (cur->m_pParent->m_color == 0)) 
			{
				cur->m_color = 1;
				vert = vert->m_pParent;
			}
			else 
			{
				if (cur->m_pRight->m_color == 0)
				{
					cur->m_pLeft->m_color = 0;
					cur->m_color = 1;
					RightRotate (root, cur);
					cur = vert->m_pParent->m_pRight;
				}

				cur->m_color = vert->m_pParent->m_color;
				vert->m_pParent->m_color = 0;
				cur->m_pRight->m_color = 0;
				LeftRotate (root, vert->m_pParent);
				vert = *root;
			}
		}
		else 
		{
			cur = vert->m_pParent->m_pLeft;
			if (cur->m_color == 1)
			{
				cur->m_color = 0;
				vert->m_pParent->m_color = 1;
				RightRotate(root, vert->m_pParent);
				cur = vert->m_pParent->m_pLeft;
			}

			if ((cur->m_pRight->m_color == 0) && (cur->m_pLeft->m_color == 0))
			{
				cur->m_color = 1;
				vert = vert->m_pParent;
			}
			else 
			{
				if (cur->m_pLeft->m_color == 0)
				{
					cur->m_pRight->m_color = 0;
					cur->m_color = 1;
					LeftRotate (root, cur);
					cur = vert->m_pParent->m_pLeft;
				}

				cur->m_color = vert->m_pParent->m_color;
				vert->m_pParent->m_color = 0;
				cur->m_pLeft->m_color = 0;
				RightRotate (root, vert->m_pParent);
				vert = *root;
			}
		}
	}

	vert->m_color = 0;
}



///successor (comes with tree minimum)
RBTree* RBTree::Successor (RBTree* box)                  // Iterative
{
	RBTree* cur;
	if (box->m_pRight != NULL)
		cur = RBTreeMinimum(box->m_pRight);
	else 
	{
		cur = box->m_pParent;
		while ((cur != NULL) && (box == cur->m_pRight))
		{
			box = cur;
			cur = cur->m_pParent;
		}
	}
	
	return (cur);
}

/// tree minimum
RBTree* RBTree::RBTreeMinimum (RBTree *box)                 // Iterative
{
	RBTree* cur;
	for (cur = box; cur->m_pLeft != NULL; cur = cur->m_pLeft);
	return (cur);
}