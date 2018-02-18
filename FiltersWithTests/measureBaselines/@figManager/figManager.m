%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
%
%   function @figManager/construct
%
%   Description:
%		manages a set of figures
%%
%   Returns:
%     this	(figManager)
%
%	 $Revision $
%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
function this = figManager(varargin)
switch nargin
    case 0
        % this = intervals
        this = init;
    case 1
        % this = intervals
        if isa(varargin{1}, 'logical')
            this = init(varargin{1})'
        else
            error('Wrong argument type')
        end
        
    otherwise
        error('Wrong number of input arguments')
end
return

function this = init(args)
if nargin < 1
    doDocking = false;
end
this.nameHash = [];
this.doDocking = doDocking;
this = class(this, 'figManager');
return
