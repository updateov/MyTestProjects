%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
%
%   function @fhrPartSet/subsref
%
%   Description:    
%	   returns a subset of fhrParts from the specified range of this
%
%	 Signatures:
%       subset = subsref(this,index)
%
%
%   Returns:
%
%	 $Revision $
%
%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
function subset = subsref(this,index)
switch index.type
case '()'
    subset = fhrPartSet(this.set{index.subs{:}});
case '{}'
    subset = this.set{index.subs{:}};
case '.'
    error('Dot not supported by fhrPart objects')
end
return