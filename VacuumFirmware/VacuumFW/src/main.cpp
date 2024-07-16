#include <Arduino.h>

// put function declarations here:
int myFunction(int, int);

void setup()
{
  Serial.begin(115200);
  pinMode(63, OUTPUT);
  pinMode(40, OUTPUT);
  pinMode(10, OUTPUT);
  pinMode(9, OUTPUT);
  digitalWrite(10, LOW);
  digitalWrite(9, LOW);
}

byte buffer[32];

void loop(){ 
  if (Serial.available())
  {
    memset(buffer, 0, sizeof(buffer));
    size_t bytesRead = Serial.readBytesUntil('\n', buffer, 32);
    if (bytesRead > 0) {
      
      String str = String((char *)buffer );
      if(str.startsWith("D")) {
      String cmd = str.substring(1, 3);
      String parameter = str.substring(4, str.length());
    
        Serial.println("COMD RECV: " + String(str.length()) +  " " + String(bytesRead) + " " + cmd + " - " + parameter);
        digitalWrite(cmd.toInt(), (parameter == "0" ? LOW : HIGH));
      }
    }
  }
}
