%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
%
%   function @uniformSignal/get
%
%   Description:    
%		accessor function
%
%   Parameters:
%		this  (extrema)
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
val = get(this.signal, prop_name);
switch prop_name
case 'indices'
    error([prop_name,' Is not a valid uniformSignal property'])
end