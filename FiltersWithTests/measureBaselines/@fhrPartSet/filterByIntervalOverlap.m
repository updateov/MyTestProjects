%%%%%%%%%%%%%%%%%%%%%%%
%
%   filterByIntervalOverlap
%
%   filter out all parts in this that have more than overlapPerc with parts
%   or intervals in intsIn.  Originally for dealing with repair.
%
%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%

function [partsOut, partsIn] = filterByIntervalOverlap(this, intsIn, overlapPerc)

    partsOut = fhrPartSet;
    partsIn = fhrPartSet;
    
    if isempty(intsIn)
        partsOut = this;
        return;
    end
    
    if (strcmp(class(intsIn), 'fhrPartSet'))
        intsIn = toIntervals(intsIn);
    end
    
    outIndex = zeros(1, length(this));
    numOut = 0;
    inIndex = zeros(1, length(this));
    numIn = 0;
    for i = 1:length(this)
        sampsOverlap = 0;
        x1 = getX1(this.set{i});
        x2 = getX2(this.set{i});
        index = 1;
        intX1 = getX1(subset(intsIn, index));
        while (intX1 <= x2)
            intX2 = getX2(subset(intsIn, index));
            if intX2 >= x1
                if intX2 < x2
                    sampsOverlap = sampsOverlap + intX2 - (max(intX1, x1)) + 1;
                else
                    sampsOverlap = sampsOverlap + x2 - (max(intX1, x1)) + 1;
                end
            end
            index = index + 1;
            if index <= size(intsIn)
                intX1 = getX1(subset(intsIn, index));
            else 
                intX1 = inf;
            end
        end
        currOverlap = sampsOverlap / (x2 - x1 + 1);
        if currOverlap < overlapPerc
            numOut = numOut + 1;
            outIndex(numOut) = i;
%             partsOut = add(partsOut, this.set{i});
        else
            numIn = numIn + 1;
            inIndex(numIn) = i;
%             partsIn = add(partsIn, this.set{i});
%             this.set{i}
%             currOverlap
        end
        
    end
    inIndex = inIndex(1:numIn);
    outIndex = outIndex(1:numOut);
    partsIn = keepIndices(this, inIndex);
    partsOut = keepIndices(this, outIndex);
    
    
return;
        
            