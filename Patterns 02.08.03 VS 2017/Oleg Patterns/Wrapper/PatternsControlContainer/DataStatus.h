#pragma once

class DataStatus
{
private:
	DataStatus() {};
	
public:
	enum data_status_code { data_status_no_data = 0, data_status_connected = 1, data_status_error = 2, data_status_recovery = 3, data_status_initialization };
};
