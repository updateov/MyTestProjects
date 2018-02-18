%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
%
%   function @fhrPartSet/sortedDisplay
%
%   Description:    
%	   sortedDisplays the fhrParts
%
%	 Parameters:
%       this              (fhrPartSet)  
%
%   Returns:
%
%	 $Revision $
%
%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
function sortedDisplay(this)
disp('fhrPartSet sortedDisplay');
disp(sprintf('name: %s', this.name));
disp(sprintf('Total number of fhrParts: %d', length(this.set)));
disp(sprintf('Total length of fhrParts: %d', calcTotalLength(this)));

fhrPartSubClasses = { ...
    'baseline', ...
    'accel', ...
    'decel', ...
    'nonaccel', ...
    'nonbaseline', ...
    'nondecel', ...
};

subClass = cell(1, length(fhrPartSubClasses));
for iClass = 1:length(fhrPartSubClasses)
    subClass{iClass} = filterByType(this, fhrPartSubClasses(iClass));
    if ~isempty(subClass{iClass})
        disp(sprintf('Type: %25s %8d parts', fhrPartSubClasses{iClass}, length(subClass{iClass})));
    end
end
for iClass = 1:length(fhrPartSubClasses)
    if ~isempty(subClass{iClass})
        disp(sprintf('\r\nType: %s', fhrPartSubClasses{iClass}));
        display(subClass{iClass});
    end
end
return

