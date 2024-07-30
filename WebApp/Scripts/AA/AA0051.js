Ext.Loader.setConfig({
    enabled: true,
    paths: {
        'WEBAPP': '/Scripts/app'
    }
});

Ext.require(['WEBAPP.utils.Common']);

Ext.onReady(function () {
    var T1Set = '';
    var T11Set = '';
    var GetInidByTuser = '/api/BC0002/GetInidByTuser';
    var MgroupComboGet = '/api/BD0003/GetMgroupCombo';
    var AngenoComboGet = '/api/BD0003/GetAngenoCombo';
    var AngenoAllComboGet = '/api/BD0003/GetAngenoAllCombo';
    var T1Name = "藥品訂單備註";

    var T1Rec = 0;
    var T1LastRec = null;
    var T11Rec = 0;
    var T11LastRec = null;
    var inidc;

    Ext.override(Ext.grid.column.Column, { menuDisabled: true });


    var AngenoStore = Ext.create('Ext.data.Store', {
        fields: ['VALUE', 'TEXT']
    });
    var AngenoAllStore = Ext.create('Ext.data.Store', {
        fields: ['VALUE', 'TEXT']
    });

    var statusStore = Ext.create('Ext.data.Store', {
        fields: ['VALUE', 'TEXT'],
        data: [
            { "VALUE": "", "TEXT": "全部" },
            { "VALUE": "Y", "TEXT": "Y 只顯示已設定" },
            { "VALUE": "N", "TEXT": "N 只顯示未設定" }
        ]
    });

    var contStore = Ext.create('Ext.data.Store', {
        fields: ['VALUE', 'TEXT'],
        data: [
            { "VALUE": "", "TEXT": "全部" },
            { "VALUE": "0", "TEXT": "0 合約" },
            { "VALUE": "2", "TEXT": "2 非合約" }
        ]
    });

    var contracnoStrore = Ext.create('Ext.data.Store', {
        fields: ['VALUE', 'TEXT'],
        data: [
            { "VALUE": "*", "TEXT": "*_不區分" },
            { "VALUE": "0Y", "TEXT": "0Y" },
            { "VALUE": "0N", "TEXT": "0N" }
        ]
    });

    function setComboData() {

        Ext.Ajax.request({
            url: AngenoComboGet,
            method: reqVal_p,
            params: { inid: inidc },
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    var tb_bsart = data.etts;
                    if (tb_bsart.length > 0) {
                        AngenoStore.removeAll();
                        AngenoStore.add({ VALUE: '', TEXT: '全部' });
                        for (var i = 0; i < tb_bsart.length; i++) {
                            AngenoStore.add({ VALUE: tb_bsart[i].VALUE, TEXT: tb_bsart[i].TEXT });
                        }
                    }
                }
            },
            failure: function (response, options) {

            }
        });
    }
    setComboData();
    function ComboData() {

        Ext.Ajax.request({
            url: AngenoAllComboGet,
            method: reqVal_p,
            // params: { inid: inidc },
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    var tb_bsart = data.etts;
                    if (tb_bsart.length > 0) {
                        AngenoAllStore.removeAll();
                        AngenoAllStore.add({ VALUE: '*', TEXT: '*_所有廠商' });
                        for (var i = 0; i < tb_bsart.length; i++) {
                            AngenoAllStore.add({ VALUE: tb_bsart[i].VALUE, TEXT: tb_bsart[i].TEXT });
                        }
                    }
                }
            },
            failure: function (response, options) {

            }
        });
    }
    ComboData();

    var mLabelWidth = 60;
    var mWidth = 180;
    var T1Query = Ext.widget({
        xtype: 'form',
        layout: 'form',
        border: false,
        autoScroll: true,
        fieldDefaults: {
            xtype: 'textfield',
            labelAlign: 'right',
            labelWidth: mLabelWidth,
            width: mWidth
        },
        items: [{
            xtype: 'panel',
            id: 'PanelP1',
            border: false,
            layout: 'hbox',
            items: [
                {
                    xtype: 'combo',
                    store: contStore,
                    displayField: 'TEXT',
                    valueField: 'VALUE',
                    queryMode: 'local',
                    fieldLabel: '合約',
                    name: 'P1',
                    id: 'P1',
                    enforceMaxLength: true,
                    padding: '0 4 0 4'
                }, {
                    xtype: 'combo',
                    store: AngenoStore,
                    displayField: 'TEXT',
                    valueField: 'VALUE',
                    queryMode: 'local',
                    fieldLabel: '廠商',
                    name: 'P2',
                    id: 'P2',
                    width: 300,
                    enforceMaxLength: true,
                    tpl: '<tpl for="."><div class="x-boundlist-item" style="height:auto;">{TEXT}&nbsp;</div></tpl>',
                    padding: '0 4 0 4'
                }, {
                    xtype: 'combo',
                    store: statusStore,
                    displayField: 'TEXT',
                    valueField: 'VALUE',
                    queryMode: 'local',
                    fieldLabel: '設定條件',
                    name: 'P3',
                    id: 'P3',
                    enforceMaxLength: true,
                    padding: '0 4 0 4'
                }, {
                    xtype: 'combo',
                    store: contracnoStrore,
                    displayField: 'TEXT',
                    valueField: 'VALUE',
                    queryMode: 'local',
                    fieldLabel: '合約碼',
                    name: 'P5',
                    id: 'P5',
                    enforceMaxLength: true,
                    padding: '0 4 0 4'
                }, {
                    xtype: 'button',
                    text: '查詢',
                    handler: function () {
                        Ext.ComponentQuery.query('panel[itemId=form]')[0].collapse();
                        //getINID(parent.parent.userId);
                        T1Load();
                        msglabel('訊息區:');
                    }
                }, {
                    xtype: 'button',
                    text: '清除',
                    handler: function () {
                        var f = this.up('form').getForm();
                        f.reset();
                        f.findField('P0').focus();
                        msglabel('訊息區:');
                    }
                }
            ]
        }]
    });
    Ext.define('T1Model', {
        extend: 'Ext.data.Model',
        fields: [' MSGRECNO', 'MSGNO', 'MSGTEXT ', 'CREATE_TIME', 'CREATE_USER', 'UPDATE_TIME', 'UPDATE_USER', 'UPDATE_IP', 'REDDISP']

    });
    var T1Store = Ext.create('WEBAPP.store.PhMailspM', {
        listeners: {
            beforeload: function (store, options) {
                var np = {
                    p1: T1Query.getForm().findField('P1').getValue(),//合約
                    p2: T1Query.getForm().findField('P2').getValue(),//廠商
                    p3: T1Query.getForm().findField('P3').getValue(),//設定條件
                    p4: inidc,
                    p5: T1Query.getForm().findField('P5').getValue(),//合約碼
                };
                Ext.apply(store.proxy.extraParams, np);
            }
        },
    });
    function T1Load() {
        T1Tool.moveFirst();
    }


    Ext.define('T2Model', {
        extend: 'Ext.data.Model',
        fields: ['MSGRECNO', 'MSGNO', 'AGEN_NO', 'M_CONTID', 'CREATE_TIME',
            'CREATE_USER', 'UPDATE_TIME', 'UPDATE_USER', 'UPDATE_IP', 'AGEN_NAMEC',
            'CONTRACNO']//ASKING_PERSON', 'RESPONDER', 'ASKING_DATE', 'CONTENT1', 'CHG_DATE', 'content', 'RESPONSE', 'RESPONSE_DATE', 'STATUS']//, 'Plant', 'PR_Create_By', 'RequestUnit', 'PR_DocType', 'Buyer', 'Status'],

    });
    var T11Store = Ext.create('WEBAPP.store.PhMailspD', {
        // autoLoad:true,

        listeners: {
            beforeload: function (store, options) {
                var np = {
                    p0: T1Form.getForm().findField('MSGNO').getValue(),    //Mail訊息編號
                    p1: T1Form.getForm().findField('MSGRECNO').getValue(), //Mail訊息流水號
                };
                Ext.apply(store.proxy.extraParams, np);
            }
        },


    });
    function T11Load() {
        T11Tool.moveFirst();
    }

    var T1Tool = Ext.create('Ext.PagingToolbar', {
        store: T1Store,
        displayInfo: true,
        border: false,
        plain: true,
        buttons: [
            {
                text: '新增', handler: function () {
                    T1Set = '/api/BD0003/MasterCreate';
                    msglabel('訊息區:');
                    setFormT1('I', '新增');
                }
            }, {
                itemId: 't1edit', text: '修改', disabled: true, handler: function () {
                    T1Set = '/api/BD0003/MasterUpdate';
                    msglabel('訊息區:');
                    setFormT1("U", '修改');
                }
            }, {
                itemId: 't1delete', text: '刪除', disabled: true,
                handler: function () {
                    msglabel('訊息區:');
                    Ext.MessageBox.confirm('刪除', '是否確定刪除？', function (btn, text) {
                        if (btn === 'yes') {
                            T1Set = '/api/BD0003/MasterDelete';
                            T1Form.getForm().findField('x').setValue('D');
                            T1Submit();
                        }
                    }
                    );
                }
            }
        ]
    });
    function setFormT1(x, t) {
        viewport.down('#t1Grid').mask();
        viewport.down('#t11Grid').mask();
        viewport.down('#form').setTitle(t + T1Name);
        viewport.down('#form').expand();
        var f = T1Form.getForm();
        //alert(parent.parent.userId);
        if (x === "I") {
            isNew = true;
            var r = Ext.create('WEBAPP.model.PhMailspM');
            T1Form.loadRecord(r);
            f.clearInvalid();
            f.findField('MSGRECNO').setReadOnly(false);
            f.findField('MSGNO').setReadOnly(false);
            //  getINID(parent.parent.userId);
            //u = f.findField('INID');
        }
        else {
            //u = f.findField('INID');
        }
        f.findField('x').setValue(x);
        //f.findField('MSGRECNO').setReadOnly(false);
        //f.findField('MSGNO').setReadOnly(false);
        f.findField('MSGTEXT').setReadOnly(false);
        f.findField('REDDISP').setReadOnly(false);
        //f.findField('INID').setVisible(true);
        T1Form.down('#t1cancel').setVisible(true);
        T1Form.down('#t1submit').setVisible(true);
        //u.focus();

    }

    var T11Tool = Ext.create('Ext.PagingToolbar', {
        store: T11Store,
        displayInfo: true,
        border: false,
        plain: true,
        buttons: [
            {
                itemId: 't11add', text: '新增', disabled: true, handler: function () {
                    T11Set = '/api/BD0003/DetailCreate';
                    setFormT11('I', '新增');
                }
                //},
                //{
                //    itemId: 't11edit', text: '修改', disabled: true, handler: function () {
                //        T11Set = '/api/BD0003/DetailUpdate';
                //        setFormT11("U", '修改');
                //    }
            }, {
                itemId: 't11delete', text: '刪除', disabled: true,
                handler: function () {
                    Ext.MessageBox.confirm('刪除', '是否確定刪除？', function (btn, text) {
                        if (btn === 'yes') {
                            T11Set = '/api/BD0003/DetailDelete';
                            T11Form.getForm().findField('x').setValue('D');
                            T11Submit();
                        }
                    }
                    );
                }
            }
        ]
    });
    function setFormT11(x, t) {

        viewport.down('#t1Grid').mask();
        viewport.down('#t11Grid').mask();
        viewport.down('#form').setTitle(t + T1Name);
        viewport.down('#form').expand();
        var f = T11Form.getForm();
        if (x === "I") {

            isNew = true;
            var r = Ext.create('WEBAPP.model.PhMailspD');
            T11Form.loadRecord(r);
            f.clearInvalid();
            f.findField('MSGRECNO').setValue(T1LastRec.data['MSGRECNO']);
            //f.findField('INID').setValue(T1LastRec.data['INID']);
            f.findField('MSGNO').setValue(T1LastRec.data['MSGNO']);

            u = f.findField("MSGNO");

            f.findField('M_CONTID0').setValue(true);
            f.findField('CONTRACNO*').setValue(true);
        }
        else {
            u = f.findField('MSGNO');
        }
        // ComboData();

        f.findField('x').setValue(x);
        f.findField('AGEN_NO').setReadOnly(false);
        f.findField('M_CONTID').setReadOnly(false);
        //f.findField('CONTRACNO').setReadOnly(false);
        T11Form.down('#t11cancel').setVisible(true);
        T11Form.down('#t11submit').setVisible(true);
        u.focus();
    }

    var T1Grid = Ext.create('Ext.grid.Panel', {
        //title: T1Name,
        store: T1Store,
        plain: true,
        loadingText: '處理中...',
        loadMask: true,
        cls: 'T1',
        dockedItems: [{
            dock: 'top',
            xtype: 'toolbar',
            layout: 'fit',
            items: [T1Query]
        }, {
            dock: 'top',
            xtype: 'toolbar',
            items: [T1Tool]
        }],
        columns: [{
            xtype: 'rownumberer'
        }, {
            text: "MAIL訊息編號",
            dataIndex: 'MSGNO',
            style: 'text-align:left',
            align: 'left',
            width: 110
        }, {
            text: "MAIL訊息流水號",
            dataIndex: 'MSGRECNO',
            style: 'text-align:left',
            align: 'left',
            width: 140
        }, {
            text: "MAIL訊息內容",
            dataIndex: 'MSGTEXT',
            style: 'text-align:left',
            align: 'left',
            width: 400
        }, {
            text: "MAIL顯示紅色字體",
            dataIndex: 'REDDISP',
            style: 'text-align:left;color:red',
            align: 'left',
            width: 400
        }, {
            header: "",
            flex: 1
        }],
        listeners: {
            click: {
                element: 'el',
                fn: function () {
                    if (T1Form.hidden === true) {
                        T1Form.setVisible(true);
                        T11Form.setVisible(false);
                    }
                }
            },
            selectionchange: function (model, records) {
                T1Rec = records.length;
                T1LastRec = records[0];
                Ext.ComponentQuery.query('panel[itemId=form]')[0].expand();
                //ComboData();
                setFormT1a();
                T11Load();
            }
        }
    });
    function setFormT1a() {

        T1Grid.down('#t1edit').setDisabled(T1Rec === 0);
        T1Grid.down('#t1delete').setDisabled(T1Rec === 0);
        if (T1LastRec) {
            isNew = false;
            T1Form.loadRecord(T1LastRec);
            var f = T1Form.getForm();
            f.findField('x').setValue('U');
            //if (T1LastRec.data['STATUS'].split(' ')[0] != 'A' && T1LastRec.data['STATUS'].split(' ')[0] != 'D') {
            // 非新增/剔退的資料就不允許修改/刪除
            T1Grid.down('#t1edit').setDisabled(false);
            T1Grid.down('#t1delete').setDisabled(false);
            //T1Grid.down('#t1audit').setDisabled(true);
            T11Grid.down('#t11add').setDisabled(false);
            // }
            //else {
            //    //T1Grid.down('#t1audit').setDisabled(false);
            //    T11Grid.down('#t11add').setDisabled(false);
            //}
            //T1Grid.down('#t1print').setDisabled(false);

        }
        else {
            T1Form.getForm().reset();
            //T1Grid.down('#t1audit').setDisabled(true);
            //T1Grid.down('#t1print').setDisabled(true);
            T11Grid.down('#t11add').setDisabled(true);
            //T11Grid.down('#t11edit').setDisabled(true);
            T11Grid.down('#t11delete').setDisabled(true);
        }
    }

    var T11Grid = Ext.create('Ext.grid.Panel', {
        store: T11Store,
        plain: true,
        loadingText: '處理中...',
        loadMask: true,
        cls: 'T1',
        dockedItems: [{
            dock: 'top',
            xtype: 'toolbar',
            items: [T11Tool]
        }],
        columns: [{
            xtype: 'rownumberer'
        }, {
            dataIndex: 'AGEN_NO',
            style: 'text-align:left',
            align: 'left',
            width: 150,
            hidden: true
        }, {
            text: "廠商代碼",
            dataIndex: 'AGEN_NAMEC',
            style: 'text-align:left',
            align: 'left',
            width: 150
        }, {
            text: "合約識別",
            dataIndex: 'M_CONTID',
            align: 'left',
            width: 100,
            renderer: function (value) {
                switch (value) {
                    case '0': return '合約'; break;
                    case '2': return '非合約'; break;
                    default: return value; break;
                }
            }
        }, {
            text: "合約碼",
            dataIndex: 'CONTRACNO',
            align: 'left',
            width: 100,
            renderer: function (value) {
                switch (value) {
                    case '*': return '不區分'; break;
                    case '0Y': return '0Y'; break;
                    case '0N': return '0N'; break;
                    default: return value; break;
                }
            }
        }, {
            header: "",
            flex: 1
        }],
        listeners: {
            click: {
                element: 'el',
                fn: function () {
                    if (T11Form.hidden === true) {
                        T1Form.setVisible(false);
                        T11Form.setVisible(true);
                    }
                }
            },
            selectionchange: function (model, records) {
                T11Rec = records.length;
                T11LastRec = records[0];
                Ext.ComponentQuery.query('panel[itemId=form]')[0].expand();

                setFormT11a();
            }
        }

    });
    function setFormT11a() {

        //T11Grid.down('#t11edit').setDisabled(T11Rec === 0);
        T11Grid.down('#t11delete').setDisabled(T11Rec === 0);
        if (T11LastRec) {
            isNew = false;
            T11Form.loadRecord(T11LastRec);
            var f = T11Form.getForm();
            f.findField('x').setValue('U');

            f.findField('M_CONTID').setReadOnly(true); //合約識別碼設定成唯讀
            f.findField('CONTRACNO').setReadOnly(true);//合約碼設定成唯讀

            //合約識別碼
            if (T11LastRec.data["M_CONTID"] == '0') //合約
            {
                f.findField('M_CONTID0').setValue(true); //M_CONTID0 合約,M_CONTID2 非合約
                f.findField('M_CONTID2').setValue(false);
            } else  //非合約
            {
                f.findField('M_CONTID0').setValue(false);
                f.findField('M_CONTID2').setValue(true);
            }

            //合約碼
            if (T11LastRec.data["CONTRACNO"] == '*') {
                f.findField('CONTRACNO*').setValue(true);
                f.findField('CONTRACNO0Y').setValue(false);
                f.findField('CONTRACNO0N').setValue(false);
            } else if (T11LastRec.data["CONTRACNO"] == '0Y') {
                f.findField('CONTRACNO*').setValue(false);
                f.findField('CONTRACNO0Y').setValue(true);
                f.findField('CONTRACNO0N').setValue(false);
            } else if (T11LastRec.data["CONTRACNO"] == '0N') {
                f.findField('CONTRACNO*').setValue(false);
                f.findField('CONTRACNO0Y').setValue(false);
                f.findField('CONTRACNO0N').setValue(true);
            }

        }
        else {
            T11Form.getForm().reset();
        }


    }

    var T1Form = Ext.widget({
        xtype: 'form',
        layout: { type: 'table', columns: 1 },
        frame: false,
        cls: 'T1b',
        title: '',
        autoScroll: true,
        bodyPadding: '5 5 0',
        fieldDefaults: {
            labelAlign: 'right',
            msgTarget: 'side',
            labelWidth: 90
        },
        defaultType: 'textfield',
        items: [{
            fieldLabel: 'Update',
            name: 'x',
            xtype: 'hidden'

        }, {
            fieldLabel: 'MAIL訊息編號',
            name: 'MSGNO',
            enforceMaxLength: true,
            maxLength: 100,
            readOnly: true
        }, {
            fieldLabel: 'MAIL訊息流水號',
            name: 'MSGRECNO',
            readOnly: true
        }, {
            xtype: 'textareafield',
            fieldLabel: 'MAIL內容',
            name: 'MSGTEXT',
            enforceMaxLength: true,
            maxLength: 4000,
            readOnly: true,
            height: 250,
            width: "100%"
        }, {
            xtype: 'textareafield',
            fieldLabel: 'MAIL顯示紅色字</br><font color=red>(多筆以";"分開)</font>',
            name: 'REDDISP',
            enforceMaxLength: true,
            maxLength: 300,
            readOnly: true,
            height: 250,
            width: "100%"
        }],
        buttons: [{
            itemId: 't1submit', text: '儲存', hidden: true,
            handler: function () {
                if (this.up('form').getForm().isValid()) {
                    var confirmSubmit = viewport.down('#form').title.substring(0, 2);
                    Ext.MessageBox.confirm(confirmSubmit, '是否確定' + confirmSubmit + '?', function (btn, text) {
                        if (btn === 'yes') {
                            T1Submit();

                        }
                    }
                    );
                }
                else {
                    Ext.Msg.alert('提醒', '輸入資料格式有誤');
                    msglabel('訊息區:輸入資料格式有誤');
                }
            }
        }, {
            itemId: 't1cancel', text: '取消', hidden: true, handler: T1Cleanup
        }]
    });
    function T1Submit() {
        var f = T1Form.getForm();
        if (f.isValid()) {
            var myMask = new Ext.LoadMask(viewport, { msg: '處理中...' });
            myMask.show();
            f.submit({
                url: T1Set,
                success: function (form, action) {
                    myMask.hide();
                    var f2 = T1Form.getForm();
                    var r = f2.getRecord();
                    switch (f2.findField("x").getValue()) {
                        case "I":
                            //T1Cleanup();
                            T1Query.getForm().reset();
                            var v = action.result.etts[0];
                            //T1Query.getForm().findField('P0').setValue(v.MSGNO);
                            T1Load();
                            //T1Cleanup();
                            msglabel('訊息區:資料新增成功');
                            break;
                        case "U":
                            var v = action.result.etts[0];
                            r.set(v);
                            r.commit();
                            msglabel('訊息區:資料修改成功');
                            break;
                        case "D":
                            T1Store.remove(r);
                            r.commit();
                            msglabel('訊息區:資料刪除成功');
                            break;
                        case "A":
                            var v = action.result.etts[0];
                            r.set(v);
                            r.commit();
                            T11Grid.down('#t11add').setDisabled(true);
                            //T11Grid.down('#t11edit').setDisabled(true);
                            T11Grid.down('#t11delete').setDisabled(true);
                            msglabel('訊息區:送審完成');
                            break;
                    }
                    T1Cleanup();
                },
                failure: function (form, action) {
                    myMask.hide();
                    switch (action.failureType) {
                        case Ext.form.action.Action.CLIENT_INVALID:
                            Ext.Msg.alert('失敗', 'Form fields may not be submitted with invalid values');
                            break;
                        case Ext.form.action.Action.CONNECT_FAILURE:
                            Ext.Msg.alert('失敗', 'Ajax communication failed');
                            break;
                        case Ext.form.action.Action.SERVER_INVALID:
                            Ext.Msg.alert('失敗', action.result.msg);
                            break;
                    }
                }
            });
        }
    }
    function T1Cleanup() {
        viewport.down('#t1Grid').unmask();
        viewport.down('#t11Grid').unmask();
        var f = T1Form.getForm();
        f.reset();
        f.getFields().each(function (fc) {
            
            if (fc.xtype == "displayfield" || fc.xtype == "textfield" || fc.xtype == "combo" || fc.xtype == "radiogroup") {
                fc.setReadOnly(true);
            } else if (fc.xtype == "datefield") {
                fc.readOnly = true;
            }
        });
        T1Form.down('#t1cancel').hide();
        T1Form.down('#t1submit').hide();
        viewport.down('#form').setTitle('瀏覽');
        Ext.ComponentQuery.query('panel[itemId=form]')[0].collapse();
        setFormT1a();
    }

    var T11Form = Ext.widget({
        hidden: true,
        xtype: 'form',
        layout: 'form',
        frame: false,
        cls: 'T2b',
        title: '',
        autoScroll: true,
        bodyPadding: '5 5 0',
        fieldDefaults: {
            labelAlign: 'right',
            msgTarget: 'side',
            labelWidth: 150
        },
        defaultType: 'textfield',
        items: [{
            fieldLabel: 'Update',
            name: 'x',
            xtype: 'hidden'
        }, {
            fieldLabel: 'MSGRECNO',
            name: 'MSGRECNO',
            xtype: 'hidden'
        }, {
            fieldLabel: 'MSGNO',
            name: 'MSGNO',
            xtype: 'hidden'
        }, {
            xtype: 'combo',
            store: AngenoAllStore,
            displayField: 'TEXT',
            valueField: 'VALUE',
            queryMode: 'local',
            fieldLabel: '廠商代碼',
            fieldCls: 'required', //設定必填欄位顏色
            allowBlank: false,    //欄位不允許空白值
            name: 'AGEN_NO',
            tpl: '<tpl for="."><div class="x-boundlist-item" style="height:auto;">{TEXT}&nbsp;</div></tpl>',
            //anyMatch: true,
            readOnly: true//            submitValue: true
        }, {
            xtype: 'radiogroup',
            fieldLabel: '合約(是/否)',
            name: 'M_CONTID',
            width: 350,
            columns: [.5, .5],
            items: [
                { boxLabel: '合約', name: 'M_CONTID', id: 'M_CONTID0', inputValue: 0, checked: true },
                { boxLabel: '非合約', name: 'M_CONTID', id: 'M_CONTID2', inputValue: 2 }
            ],
            listeners: {
                change: function (combo, newValue, oldValue, eOpts) {
                    if (T11Form.getForm().findField("x").getValue() == 'I') //新增時才觸發change事件
                    {
                        if (newValue.M_CONTID == 0) //合約識別碼=合約     
                        {
                            T11Form.getForm().findField('CONTRACNO').setReadOnly(true);
                            T11Form.getForm().findField('CONTRACNO*').setValue(true); //合約碼=不區分            
                        }
                        else {
                            T11Form.getForm().findField('CONTRACNO').setReadOnly(false);
                        }
                    }
                }
            },
        }, {
            xtype: 'radiogroup',
            fieldLabel: '合約碼',
            name: 'CONTRACNO',
            width: 350,
            items: [
                { boxLabel: '不區分', name: 'CONTRACNO', id: 'CONTRACNO*', inputValue: '*', checked: true },
                { boxLabel: '0Y', name: 'CONTRACNO', id: 'CONTRACNO0Y', inputValue: '0Y' },
                { boxLabel: '0N', name: 'CONTRACNO', id: 'CONTRACNO0N', inputValue: '0N' }
            ],

        }],
        buttons: [{
            itemId: 't11submit', text: '加入廠商', hidden: true,
            handler: function () {
                if (this.up('form').getForm().isValid()) {
                    var confirmSubmit = viewport.down('#form').title.substring(0, 2);
                    Ext.MessageBox.confirm(confirmSubmit, '是否確定' + confirmSubmit + '?', function (btn, text) {
                        if (btn === 'yes') {
                            T11Submit();
                        }
                    }
                    );
                }
                else {
                    Ext.Msg.alert('提醒', '輸入資料格式有誤');
                    msglabel('訊息區:輸入資料格式有誤');
                }
            }
        }, {
            itemId: 't11cancel', text: '取消', hidden: true, handler: T11Cleanup
        }]
    });
    function T11Submit() {
        var f = T11Form.getForm();
        if (f.isValid()) {
            var myMask = new Ext.LoadMask(viewport, { msg: '處理中...' });
            myMask.show();
            f.submit({
                url: T11Set,
                success: function (form, action) {
                    myMask.hide();
                    var f2 = T11Form.getForm();
                    var r = f2.getRecord();
                    switch (f2.findField("x").getValue()) {
                        case "I":
                            var v = action.result.etts[0];
                            r.set(v);
                            T11Store.insert(0, r);
                            r.commit();
                            msglabel('訊息區:資料新增成功');
                            break;
                        case "U":
                            var v = action.result.etts[0];
                            r.set(v);
                            r.commit();
                            msglabel('訊息區:資料修改成功');
                            break;
                        case "D":
                            T11Store.remove(r);
                            r.commit();
                            msglabel('訊息區:資料刪除成功');
                            break;
                    }
                    T11Cleanup();
                },
                failure: function (form, action) {
                    myMask.hide();
                    switch (action.failureType) {
                        case Ext.form.action.Action.CLIENT_INVALID:
                            Ext.Msg.alert('失敗', 'Form fields may not be submitted with invalid values');
                            break;
                        case Ext.form.action.Action.CONNECT_FAILURE:
                            Ext.Msg.alert('失敗', 'Ajax communication failed');
                            break;
                        case Ext.form.action.Action.SERVER_INVALID:
                            Ext.Msg.alert('失敗', action.result.msg);
                            break;
                    }
                }
            });
        }
    }
    function T11Cleanup() {
        viewport.down('#t1Grid').unmask();
        viewport.down('#t11Grid').unmask();
        var f = T11Form.getForm();
        f.reset();
        f.getFields().each(function (fc) {
            
            if (fc.xtype == "displayfield" || fc.xtype == "textfield" || fc.xtype == "radiogroup") {
                fc.setReadOnly(true);
            } else if (fc.xtype == "combo" || fc.xtype == "datefield") {
                fc.readOnly = true;
            }
        });
        T11Form.down('#t11cancel').hide();
        T11Form.down('#t11submit').hide();
        viewport.down('#form').setTitle('瀏覽');
        Ext.ComponentQuery.query('panel[itemId=form]')[0].collapse();
        setFormT11a();
    }

    //function getINID(parTuser) {
    //    Ext.Ajax.request({
    //        url: GetInidByTuser,
    //        method: reqVal_p,
    //        params: { tuser: parTuser },
    //        success: function (response) {
    //            var data = Ext.decode(response.responseText);
    //            if (data.success) {
    //                var tb_data = data.etts;
    //                if (tb_data.length > 0) {

    //                    inidc = tb_data[0].INID
    //                    setComboData(inidc);
    //                    T1Form.getForm().findField('INID').setValue(tb_data[0].INID);
    //                    //T1Form.getForm().findField('INIDNAME').setValue(tb_data[0].INID_NAME);
    //                }
    //                else {
    //                    //T1Form.getForm().findField('APP_INID').setValue('');
    //                    T1Form.getForm().findField('INID').setValue('');
    //                    //T1Form.getForm().findField('INIDNAME').setValue('');
    //                }
    //            }
    //        },
    //        failure: function (response, options) {

    //        }
    //    });
    //}



    var viewport = Ext.create('Ext.Viewport', {
        renderTo: body,
        layout: {
            type: 'border',
            padding: 0
        },
        defaults: {
            split: true
        },
        items: [
            {
                itemId: 'tGrid',
                region: 'center',
                layout: 'fit',
                collapsible: false,
                title: '',
                border: false,
                items: [{
                    //  xtype:'container',
                    region: 'center',
                    layout: {
                        type: 'border',
                        padding: 0
                    },
                    collapsible: false,
                    title: '',
                    split: true,
                    width: '80%',
                    flex: 1,
                    minWidth: 50,
                    minHeight: 140,
                    items: [
                        {
                            itemId: 't1Grid',
                            region: 'center',
                            layout: 'fit',
                            collapsible: false,
                            title: '',
                            border: false,
                            items: [T1Grid]
                        }, {
                            itemId: 't11Grid',
                            region: 'south',
                            layout: 'fit',
                            collapsible: false,
                            title: '',
                            height: '50%',
                            split: true,
                            items: [T11Grid]
                        }
                    ]
                }]
            },
            {
                itemId: 'form',
                region: 'east',
                collapsible: true,
                floatable: true,
                width: 350,
                title: '瀏覽',
                border: false,
                collapsed: true,
                layout: {
                    type: 'fit',
                    padding: 5,
                    align: 'stretch'
                },
                items: [T1Form, T11Form]
            }
        ]
    });
    //ComboData();
    //getINID(parent.parent.userId);

    T1Query.getForm().findField('P1').focus();
    T1Query.getForm().findField("P5").select('*');
});