%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
%
%   function @fhrPartSet/filterByX2
%
%   Description:    
%	   filter out any parts whose ending indexies are greater than the specified x2
%
%	 Parameters:
%       this              (fhrPartSet)  
%       x2                (double)
%       filterGreater     (logical)
%   Returns:
%
%	 $Revision $
%
%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
function [setLessThanEq, setGreaterThan] = filterByX2(varargin)
% 1) Checking inputs---------------------------------------
if isa(varargin{1},'fhrPartSet') & isa(varargin{2},'double')
else
    error('Wrong argument type');
end
% % 2) Convert to 3 input arguments---------------------------
% switch nargin
%     case 2 %default
%         varargin{3} = true;
%     case 3
%         if isa(varargin{3},'logical')
%         else
%             error('Wrong argument type');
%         end
%     otherwise
%         error('Wrong number of input arguments');   
% end
% 3) Call the function-------------------------------------
[setLessThanEq, setGreaterThan] = filterByX2_Impl(varargin{1},varargin{2});
return
%==============================================================================
function [setLessThanEq, setGreaterThan] = filterByX2_Impl(this, x2 )
toDeleteIndices = [];
setLessThanEq = fhrPartSet;
setGreaterThan = fhrPartSet; 
for i = 1:length(this.set)
%     switch filterGreater
%         case true
%             if getX2(this.set{i}) > x2
%                 toDeleteIndices(end+1) = i;
%             end
%         case false
%             if getX2(this.set{i}) <= x2
%                 toDeleteIndices(end+1) = i;
%             end
%     end
    if getX2(this.set{i}) <= x2
         setLessThanEq  = add(setLessThanEq , this.set{i});
    else
         setGreaterThan = add(setGreaterThan, this.set{i});
    end
end
this = fhrPartSet(this.set{setdiff(1:length(this), toDeleteIndices)});
return
