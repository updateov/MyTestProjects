%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
%
%   function @intervals/get
%
%   Description:    
%		accessor function
%
%   Parameters:
%		this  (intervals)
%		
%   Returns:
%     val
%
%	 $Revision $
%
%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
function val = get(this, prop_name)
% GET Get asset properties from the specified object
% and return the value
switch prop_name
case 'x1'
    val = this.x1;
case 'x2'
    val = this.x2;
otherwise
    error([prop_name,' is not a valid property'])
end