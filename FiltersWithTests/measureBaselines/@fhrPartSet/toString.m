%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
%
%   function @fhrPartSet/toString
%
%   Description:    
%	   toStrings the fhrParts
%
%	 Parameters:
%       this              (fhrPartSet)  
%
%   Returns:
%
%	 $Revision $
%
%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
function out = toString(this)
strBuf = '';
for i = 1:length(this.set)
    if (i > 1) & (getX1(this.set{i}) < getX2(this.set{i-1}))
        overlapStr = ' **overlap**';
    else
        overlapStr = '';
    end
    strBuf = [strBuf sprintf('%-3d %s%s\n', i, toString(this.set{i}), overlapStr)];
end
headerStr = sprintf('fhrPartSet display\nname: %s\nnumber of fhrParts: %d\nlength of fhrParts: %d\n', ... 
                    this.name, ...
                    length(this), ...
                    calcTotalLength(this));
out = sprintf('%s\n%s', headerStr, strBuf);
return


