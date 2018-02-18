%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
%
%   function @fhrPartSet/construct
%
%   Description:    
%		constructs set of fhrPart
%
%	 Signatures
%		this = fhrPartSet     creates empty intervals set this =
%		fhrPartSet(fhr_part_vector)     creates from ud_save 
%              fhr_part_vector structure array
%		this = rawFhrPartSet     creates from raw set of fhrPart
%
%   Returns:
%     this	(fhrPartSet)
%
%	 $Revision $
%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
function this = fhrPartSet(varargin)
%   this = fhrPartSet(rawFhrPartSet);
if nargin > 0
    if isa(varargin{1},'fhrPart')
        this = init;
        this.set = varargin;
        return
    end
end
switch nargin
    case 0
        % this = fhrPartSet
        this = init;
        
    case 1
        %   this = fhrPartSet(fhr_part_vector);
        if isa(varargin{1},'struct')
            this = init;
            this = fromFhrPartVector(this, varargin{1});
        else
            error('Wrong argument type')
        end
        
    otherwise
        error('Wrong argument type')   
end
return

function this = init() 
USE_FHR_PART2 = 0;  
%%%%%%%%%%%%%
% Note July 2008
%
% Updated many of the fhrPart types (fhrPart2, decel2 etc...).  These
% contain additional information and facilitate I/O with C++.  A second
% fhrPartParser (fhrPartParser2) was created as C++ output is written to
% MATLAB as fhr_part_vectors.  Ideally we could use an fhrPartSet2 class, 
% but because much of the test code (filtering
% and manipulating of fhrPartSets) reconstructs fhrPartSets from the input
% fhrPartSets, the fhrPartParser information is usually lost. .  Rather than
% redoing all the filtering and test code that manipulates fhrPartSets, the
% default parser is changed here.  If need to deal with old fhrPart data,
% please set USE_FHR_PART back to 0.

this.name = [];
this.set = {};
if USE_FHR_PART2
    this.fhrPartParser = fhrPartParser2;
else
    this.fhrPartParser = fhrPartParser;
end
this = class(this, 'fhrPartSet');
return
