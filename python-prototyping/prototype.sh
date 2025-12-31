while true; do
    inotifywait -e close_write ./python-prototyping/main.py
    python3 ./python-prototyping/main.py
done
