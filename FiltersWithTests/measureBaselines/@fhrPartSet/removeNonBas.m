function this = removeNonBas(this)

    keep = ones(1, length(this.set));
    
    for i = 1:length(this.set)
        if isa(this.set{i}, 'nonBaseline') || isa(this.set{i}, 'nonBaseline2')
            keep(i) = 0;
        end
    end
    
    keepInd = find(keep == 1);
    this = keepIndices(this, keepInd);
    
return;