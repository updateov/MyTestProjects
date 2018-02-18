%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
%
%   function @uniformSignal/display
%
%   Description:    
%		displays a uniformSignal
%
%   Parameters:
%		this 				(uniformSignal)
%
%   Returns:
%     none
%
%	History:
%		14 Aug 2001			PAW
%		20 Sept 2001		PAW 	created as class
%
%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
function display(this)
% substStruct.type = '.';
% substStruct.subs = 'name';
% temp = subsref(this, substStruct);

disp(['name ' get(this, 'name')]);
disp(['Fs ' num2str(get(this, 'Fs'))]);
disp(['Extent ' num2str(get(this, 'extent'))]);
disp(['Samples: (size) ' num2str(length(get(this, 'samples')))]);
return
