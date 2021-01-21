const {ref, computed} = Vue;

const ProposeCalendar = {
    components: {
        'vue-cal': vuecal
    },
    setup() {
        const vuecal = ref(null);
        const rawEvents = ref({});
        const deleteFns = ref({});
        
        const occurences = computed(() =>
            Object.values(rawEvents.value).map(ev => ({id: ev._eid, start: ev.start, end: ev.end}))
        );

        const eventUpdated = (event, deleteFn) => {
            event = event.event || event;
            rawEvents.value[event._eid] = event;
            if (deleteFn) {
                deleteFns.value[event._eid] = deleteFn;
            }
            return true;
        };

        const eventRemoved = (event) => {
            delete deleteFns.value[event._eid];
            delete rawEvents.value[event._eid];
        };

        const handleCellDblclick = (time) => {
            time.setMinutes(Math.round(time.getMinutes()/15) * 15, 0, 0);
            vuecal.value.createEvent(time, 120);
        };

        const handleEventDblclick = (event, domEvent) => {
            if (deleteFns.value[event._eid]) {
                deleteFns.value[event._eid]();
            }
            
            domEvent.stopPropagation();
        };

        return {
            vuecal,
            occurences,
            eventUpdated,
            eventRemoved,
            handleCellDblclick,
            handleEventDblclick
        };
    }
}

const app = Vue.createApp(ProposeCalendar);
app.mount("#propose-calendar");