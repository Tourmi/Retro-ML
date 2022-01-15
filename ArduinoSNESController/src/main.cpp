#include <Arduino.h>

#define A 22
#define B 23
#define X 24
#define Y 25
#define l 26 //Left
#define r 27 //Right
#define u 28 //Up
#define d 29 //Down
#define L 30 //Left Shoulder
#define R 31 //Right Shoulder
#define S 32 //Start
#define s 33 //Select

byte* bytes = new byte[2];

void setup() {
  Serial.begin(9600);
  for(int i = A; i <= s; i++) {
    pinMode(i, OUTPUT);
  }
}

void loop() {
  Serial.readBytes(bytes, 2);

  //first 8 buttons
  for (int i = 0; i < 8; i++) {
    digitalWrite(i + A, bytes[0] & 1 << i ? HIGH : LOW);
  }

  //last 4
  for (int i = 0; i < 4; i++) {
    digitalWrite(i + 8 + A, bytes[1] & 1 << i ? HIGH : LOW);
  }
}