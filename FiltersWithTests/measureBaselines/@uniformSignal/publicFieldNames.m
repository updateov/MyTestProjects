%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
%
%   function @uniformSignal/publicFieldNames
%
%   Description:    
%		returns a cell array of publicly accessible field names.  Note that
%		the parent class 'indices' field is not accessible
%
%   Parameters:
%		this		the signal
%
%   Returns:
%     out the field to return
%
%	History:
%		20 Sept 2001		PAW 	created
%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
function out = publicFieldNames(this)
out = {'name', 'samples', 'Fs', 'extent'};
return