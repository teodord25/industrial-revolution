while true; do
    inotifywait -e close_write ./$1
    python3 ./$1
done
