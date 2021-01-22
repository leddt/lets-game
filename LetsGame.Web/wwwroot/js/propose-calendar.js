const {ref, computed} = Vue;

const cellClickPrecision = 30;
const eventLength = 60

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

        const handleCellClick = (time) => {
            time.setMinutes(Math.floor(time.getMinutes()/cellClickPrecision) * cellClickPrecision, 0, 0);
            vuecal.value.createEvent(time, eventLength);
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
            handleCellClick,
            handleEventDblclick
        };
    }
}

const app = Vue.createApp(ProposeCalendar);
app.mount("#propose-calendar");