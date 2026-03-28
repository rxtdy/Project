<template>
  <v-app>
    <v-app-bar app color="primary" dark>
      <v-toolbar-title>Система управления ВУЗом</v-toolbar-title>
      <v-spacer></v-spacer>
      <v-btn to="/" text @click="resetApp">Обновить</v-btn>
    </v-app-bar>

    <v-main class="bg-grey-lighten-4">
      <keep-alive include="GroupsTable">
        <router-view :key="componentKey"></router-view>
      </keep-alive>
    </v-main>
  </v-app>
</template>

<script>
export default {
  name: 'App',
  data() {
    return {
      componentKey: 0,
    };
  },
  methods: {
    resetApp() {
      this.componentKey = Date.now(); 
    },
}};

</script>

<style>
.v-application {
  font-family: 'Roboto', sans-serif;
}
</style>