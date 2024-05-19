<template>
  <div class="container mx-auto p-4">
    <h1 class="text-2xl font-bold mb-4">Tenant Search App</h1>
    <div class="mb-4">
      <label for="tenant" class="block mb-2">Select Tenant:</label>
      <select v-model="selectedTenant" id="tenant" class="p-2 border rounded w-full">
        <option value="a">Tenant A</option>
        <option value="b">Tenant B</option>
        <option value="c">Tenant C</option>
      </select>
    </div>

    <div class="mb-4">
      <label for="search" class="block mb-2">Search:</label>
      <input
        type="text"
        id="search"
        v-model="query"
        @input="searchCompanies"
        placeholder="Enter company name"
        class="p-2 border rounded w-full"
      />
    </div>

    <div id="results">
      <div v-if="companies.length === 0 && query" class="text-red-500">No results found</div>
      <div v-for="company in companies" :key="company.companyName" class="company p-2 border-b">
        <strong>{{ company.companyName }}</strong><br />
        {{ company.address }}
      </div>
    </div>
  </div>
</template>

<script>
export default {
  data() {
    return {
      selectedTenant: 'a',
      query: '',
      companies: [],
      searchTimeout: null
    };
  },
  methods: {
    searchCompanies() {
      clearTimeout(this.searchTimeout);
      this.searchTimeout = setTimeout(() => {
        if (this.query) {
          fetch(`${process.env.VUE_APP_HOSTNAME}/tenants/${this.selectedTenant}/companies?q=${this.query}`)
            .then(response => response.json())
            .then(data => {
              this.companies = data;
            })
            .catch(error => {
              console.error('Error fetching data:', error);
            });
        } else {
          this.companies = [];
        }
      }, 500);
    }
  }
};
</script>

<style scoped>
.container {
  max-width: 600px;
}
</style>