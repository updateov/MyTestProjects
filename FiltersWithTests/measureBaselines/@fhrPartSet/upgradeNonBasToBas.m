function this = upgradeNonBasToBas(this)

    % only works for fhrPart2 - nonBaseline2 to baseline2
    % otherwise use mhPartParser for fhrPart
    for i = 1:length(this.set)
        b = this.set{i};
        if isa(b, 'nonBaseline2')
            this.set{i} = toBas(b);
        end
    end
    
return;
            