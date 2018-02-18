%%%%%%%%%%%%%%%%%%%%%
%
%   filterByIsLate - only applies to fhrPart2 - checks 'isVariable' flag
%   Can get prolonged that were variable
%
%%%%%%%%%%%%%%%

function this = filterByIsVariable(this)

    if hasFhrPart2(this)
        
        keep = zeros(1, length(this));
        n = 0;
        for i = 1:length(this)
            if (isVariable(this.set{i}))
%             if isa(this.set{i}, 'decel2') && (isVariable(this.set{i}))
                n = n + 1;
                keep(n) = i;
            end
        end
        keep = keep(1:n);
    
        this = keepIndices(this, keep);
    end
    
return;
    