while inotifywait -e modify ./python-prototyping/main.py;
    do python3 ./python-prototyping/main.py;
done
