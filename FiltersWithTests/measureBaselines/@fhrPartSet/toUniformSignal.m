%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
%
%   function @fhrPartSet/toUniformSignal
%
%   Description:    
%	   converts this to a uniformSignal.  Gaps are set to 0. Individual
%	   Values in each fhrPart interval delegated to corresonding fhrPart
%	   class.
%
%	 Parameters:
%       this              (fhrPartSet)  
%
%   Returns:
%       out               (uniformSignal) 
%
%	 $Revision $
%
%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
function out = toUniformSignal(this, gapValue, fhr)
out = signal([], [], [], [], []);
if nargin == 1
    gapValue = 0;
end
if isempty(this)
    out = uniformSignal(ones(1, length(fhr))*gapValue);
    return;
end
if strcmp(gapValue, 'join')
    doJoin = true;
else
    doJoin = false;
end
lastX1 = 0;
lastY2 = 0;
%  debug
% figure;
% hold on;
%  debug
for i = 1:length(this.set)
    currPart = this.set{i};
    gapX1 = lastX1+1:getX1(currPart)-1;
    if doJoin   
        if (length(gapX1)) == 0
            gapX2 = [];
        else
            currY1 = getY1(currPart);
            if i == 1
                lastY2 = currY1;
            end
            inc = (currY1-lastY2)/(length(gapX1)+1);
            if inc == 0
                gapX2 = ones(1, length(gapX1)) * currY1;
            else
                gapX2 = lastY2:inc:currY1;
            end
        end
        lastY2 = getY2(currPart);
    elseif (length(gapX1)) == 0
        gapX2 = [];
    else
        gapX2 = ones(1, length(gapX1)) * gapValue;
    end
    gap = signal(gapX2, gapX1);
    out = merge(out, gap);
    
    %  debug
%        out
%        withGapLength = length(out)
%        plot(getSamples(out));
    %  debug
    
    currSignal = toSignal(currPart, fhr);
    out = merge(out, currSignal);
    
    lastX1 = getX2(currPart);
    
    %  debug
%         currPart
%         sigLength = length(out)
%         doDummy = -8;
%        out
%        sigLength = length(out)
%        plot(getSamples(out));
%        plot(fhr(1:lastX1), 'r');
    %  debug
    
end

if doJoin
    gapValue = lastY2;
end
lenDiff = length(fhr) - length(out);
if lenDiff > 0
    endGap = ones(1, length(fhr) - length(out)) * gapValue;
    gap = signal(endGap, length(out)+1:length(fhr));
    out = merge(out, gap);
end

out = toUniformSignal(out);
return
