import Vue from 'vue';
import VueRouter from 'vue-router';
import GroupsTable from '../views/GroupsTable.vue';
import StudentsTable from '../views/StudentsTable.vue';

Vue.use(VueRouter);

const routes = [
  { 
    path: '/', 
    name: 'Groups', 
    component: GroupsTable,
    props: route => ({ 
      page: parseInt(route.query.page) || 1, 
      pageSize: parseInt(route.query.pageSize) || 10 
    })
  },
  { 
    path: '/group/:id', 
    name: 'Students', 
    component: StudentsTable,
    props: route => ({ 
      id: Number(route.params.id),
      groupName: route.query.groupName || '',
    })
  }
];

const router = new VueRouter({
  mode: 'history',
  routes
});

export default router;