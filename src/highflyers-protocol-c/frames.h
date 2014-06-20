#ifndef HIGHFLYERS_PROTOCOL_FRAMES_H
#define HIGHFLYERS_PROTOCOL_FRAMES_H

#include "types.h"

typedef enum
{
	T_TestStruct
} FrameTypes;

typedef struct
{
	uint32 Field1;
	double Field2;
	bool Field2_enabled;
	byte Field3;
	int Field4;
	bool Field4_enabled;
} TestStruct;

void frame_preparse_data (const byte* data, bool* output, int field_count);

TestStruct* TestStruct_parse (const byte* data, int size);
int TestStruct_current_size (const TestStruct* value);
void TestStruct_serialize (const TestStruct* value, byte* output);

#endif
