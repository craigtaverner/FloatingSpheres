using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Neo4j.Driver.V1;
using UnityEngine.UI;

namespace FloatingSpheres
{
    public class Neo4jManager : MonoBehaviour
    {
        public string boltAddress = "bolt://localhost:7687";
        public string username = "neo4j";
        public string password = "neo4j";
        public string cypher = "MATCH (n) WHERE exists(n.location) RETURN n";

        public InputField boltAddressField;
        public InputField usernameField;
        public InputField passwordField;
        public InputField cypherField;
        public Button openLoginButton;
        public Button closeLoginButton;
        public Button loginButton;
        public Button importButton;
        public Canvas loginMenuCanvas;

        private IDriver driver;
        private ISession session;

        void Start()
        {
            loginMenuCanvas = CheckCanvas(loginMenuCanvas, this.transform, "Login to Neo4j Menu");
            openLoginButton = CheckButton(openLoginButton, this.transform, "Open Login Button");
            closeLoginButton = CheckButton(openLoginButton, loginMenuCanvas.transform, "Close Login Button");
            loginButton = CheckButton(loginButton, loginMenuCanvas.transform, "Login Button");
            importButton = CheckButton(importButton, this.transform, "Import Button");
            usernameField = CheckInputField(usernameField, loginMenuCanvas.transform, "Username InputField");
            passwordField = CheckInputField(passwordField, loginMenuCanvas.transform, "Password InputField");
            boltAddressField = CheckInputField(boltAddressField, loginMenuCanvas.transform, "Bolt Address InputField");
            cypherField = CheckInputField(cypherField, this.transform, "Cypher InputField");
            boltAddressField.text = boltAddress;
            usernameField.text = username;
            passwordField.text = password;
            cypherField.text = cypher;
            this.driver = null;
            this.session = null;
            CloseLoginMenu();
        }

        private Canvas CheckCanvas(Canvas obj, Transform parent, string name)
        {
            if (obj == null)
            {
                obj = parent.Find(name).GetComponent<Canvas>();
            }
            return obj;
        }

        private InputField CheckInputField(InputField obj, Transform parent, string name)
        {
            if (obj == null)
            {
                obj = parent.Find(name).GetComponent<InputField>();
            }
            return obj;
        }

        private Button CheckButton(Button obj, Transform parent, string name)
        {
            if (obj == null)
            {
                obj = parent.Find(name).GetComponent<Button>();
            }
            return obj;
        }

        public void Login()
        {
            Logout();
            this.driver = GraphDatabase.Driver(boltAddress, AuthTokens.Basic(username, password));
            using (ISession session = driver.Session())
            {
                IStatementResult result = session.Run("CREATE (n) RETURN n");
            }
            driver.Dispose();
        }

        public void Logout()
        {
            if (session != null)
            {
                session.Dispose();
                session = null;
            }
            if (driver != null)
            {
                driver.Dispose();
                driver = null;
            }
        }

        public void ExecuteQuery()
        {
            SetCypherQuery();
            if (session == null)
            {
                session = driver.Session();
            }
            IStatementResult result = session.Run(cypher);
            foreach (IRecord record in result)
            {
                Debug.Log(record.ToString());
            }
        }

        public void OpenLoginMenu()
        {
            this.loginMenuCanvas.gameObject.SetActive(true);
        }
        public void CloseLoginMenu()
        {
            this.loginMenuCanvas.gameObject.SetActive(false);
        }
        public void SetBoltAddress()
        {
            this.boltAddress = boltAddressField.text;
        }
        public void SetUsername()
        {
            this.username = usernameField.text;
        }
        public void SetPassword()
        {
            this.password = passwordField.text;
        }
        public void SetCypherQuery()
        {
            this.cypher = cypherField.text;
        }

    }
}
