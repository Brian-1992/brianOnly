Ext.Loader.setConfig({
    enabled: true,
    paths: {
        'WEBAPP': '/Scripts/app'
    }
});

Ext.require(['WEBAPP.utils.Common']);


// -- 建立下拉選單 - -
var months = Ext.create('Ext.data.Store', {
    fields: ['abbr', 'name'],
    data: [
        { "TEXT": "1", "VALUE": "1" },
        { "TEXT": "2", "VALUE": "2" },
        { "TEXT": "3", "VALUE": "3" },
        { "TEXT": "4", "VALUE": "4" },
        { "TEXT": "5", "VALUE": "5" },
        { "TEXT": "6", "VALUE": "6" },
        { "TEXT": "7", "VALUE": "7" },
        { "TEXT": "8", "VALUE": "8" },
        { "TEXT": "9", "VALUE": "9" },
        { "TEXT": "10", "VALUE": "10" },
        { "TEXT": "11", "VALUE": "11" },
        { "TEXT": "12", "VALUE": "12" }
    ]
});




Ext.onReady(function () {
    var T1Get = '/api/BG0002/All'; // 查詢(改為於store定義)
    var T1Set = ''; // 新增/修改/刪除
    var T1Name = "衛材非合約累計進貨金額查詢";
    var T1Rec = 0;
    var T1LastRec = null;

    // -- 共用函式區開始 -- 
    // GMT 標準時間(格林威治平均時間)轉西元yyyy/MM/dd
    function gmtTimeToYmd(gmtDate) {
        var sRtn = "";
        try {
            sRtn = gmtDate.getFullYear() + '/' + (gmtDate.getMonth() + 1) + '/' + gmtDate.getDate();
        }
        catch (e) {
        }
        return sRtn;
    } // 
    // -- 日期相加 相減 加減 --
    function getAddDate(addDays) {
        var dt = new Date();
        dt.setDate(dt.getDate() + addDays);
        return dt;
    } // 
    // -- 設定 Combo 選取 預設值 --
    // 使用範例 var combo = T1Query.getForm().findField('WH_NO'); setComboDefaultValue(combo, "560000");
    function setComboDefaultValue(comboBox, value) {
        var store = comboBox.store;
        var valueField = comboBox.valueField;
        var displayField = comboBox.displayField;

        var recordNumber = store.findExact(valueField, value, 0);

        if (recordNumber == -1)
            return -1;

        var displayValue = store.getAt(recordNumber).data[displayField];
        comboBox.setValue(value);
        comboBox.setRawValue(displayValue);
        comboBox.selectedIndex = recordNumber;
        return recordNumber;
    }
    // -- 共用函式區結束 -- 


    // 庫房清單
    var st_wh_no = Ext.create('Ext.data.Store', {
        fields: ['VALUE', 'COMBITEM']
    });
    // 物料分類
    var st_matclass = Ext.create('Ext.data.Store', {
        fields: ['VALUE', 'COMBITEM']
    });
    function setComboData() {
        // 庫房清單
        Ext.Ajax.request({
            url: '../../../api/BG0002/GetWhnoCombo',
            method: reqVal_g,
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    var wh_nos = data.etts;
                    if (wh_nos.length > 0) {
                        st_wh_no.add({ VALUE: '', COMBITEM: '' });
                        var wh_no = null;
                        for (var i = 0; i < wh_nos.length; i++) {
                            if (wh_no == null) {
                                wh_no = wh_nos[i].VALUE;
                            }
                            st_wh_no.add({ VALUE: wh_nos[i].VALUE, COMBITEM: wh_nos[i].TEXT });
                        }
                        setComboDefaultValue(T1Query.getForm().findField('WH_NO'), session['Inid']);   // 載入預設選項
                        // combo.select(combo.getStore().getAt(0));    // 若有資料combo選取第一筆資料
                        // setMmcodeCombo(wh_no);
                    }
                    else {
                        Ext.MessageBox.alert('錯誤', '查不到你所屬庫房資料');
                        msglabel('訊息區:查不到你所屬庫房資料');
                    }
                }
            },
            failure: function (response, options) {
            }
        });

        // 物料分類
        Ext.Ajax.request({
            url: '../../../api/BG0002/GetMatClassCombo',
            method: reqVal_g,
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    var tb_matclass = data.etts;
                    if (tb_matclass.length > 0) {
                        //st_matclass.add({ VALUE: '', COMBITEM: '' });
                        for (var i = 0; i < tb_matclass.length; i++) {
                            st_matclass.add({ VALUE: tb_matclass[i].VALUE, COMBITEM: tb_matclass[i].TEXT });
                        }
                        if (tb_matclass.length > 0) // 有資料的話，預設第一筆
                        {   
                            T1Query.getForm().findField('MAT_CLASS').setValue(tb_matclass[0].VALUE);
                            // T1Query.getForm().findField('MAT_CLASS').setDisabled(false);
                        }
                    }
                }
            },
            failure: function (response, options) {

            }
        });
    }
    setComboData();
    

    function showReport() { // 顯示報表
        if (!win) {
            if (
                T1Query.getForm().findField('WH_NO').getValue() != null &&      // 庫房別 
                T1Query.getForm().findField('MAT_CLASS').getValue() != null &&  // 物料類別
                T1Query.getForm().findField('DATA_YM_START').getValue() != null &&  // 年月
                T1Query.getForm().findField('DATA_YM_END').getValue() != null &&  // 年月
                true
            ) {
                var url = "";
                url += reportUrl + '?'
                    + '&WH_NO=' + T1Query.getForm().findField('WH_NO').getValue()                   // 庫房別 
                    + '&MAT_CLASS=' + T1Query.getForm().findField('MAT_CLASS').getValue()           // 物料類別
                    + '&MAT_CLASS_NAME=' + encodeURIComponent(T1Query.getForm().findField('MAT_CLASS').rawValue)          // 物料類別名稱
                    + '&DATA_YM_START=' + gmtTimeToYmd(T1Query.getForm().findField('DATA_YM_START').getValue())         // 年月
                    + '&DATA_YM_END=' + gmtTimeToYmd(T1Query.getForm().findField('DATA_YM_END').getValue())   // T1Query.getForm().findField('DATA_YM_END').getValue()       // 年月
                    + '&RADIO_BUTTON=' + T1Query.getForm().findField('RADIO_BUTTON').getChecked()[0].inputValue       // 年月
                    + '&inid=' + session['Inid'] + ' ' + session['InidName'];                           // 使用單位:庫房別 inid='560000 中央庫房'
                var winform = Ext.create('Ext.form.Panel', {
                    id: 'iframeReport',
                    //height: '100%', width: '100%',
                    layout: 'fit',
                    closable: false,
                    html: '<iframe src="' + url + '" id="mainContent" name="mainContent" width="100%" height="100%" frameborder="0" style="background-color:#FFFFFF"></iframe>',
                    buttons: [{
                        text: '關閉',
                        handler: function () {
                            this.up('window').destroy();
                        }
                    }]
                });
                var win = GetPopWin(viewport, winform, '', viewport.width - 20, viewport.height - 20);
                win.show();
            }
            else
            {
                Ext.Msg.alert('提醒', '庫房別 與 物料類別 與 年月 必填');
                msglabel('訊息區:庫房別 與 物料類別 與 年月 必填');
            }
        }
    }

    Ext.override(Ext.grid.column.Column, { menuDisabled: true });

    var form_recStatus = Ext.create('Ext.data.Store', {
        fields: ['KEYCODE', 'VALUE'],
        data: [
            { "KEYCODE": "A", "NAME": "啟用", "COMBITEM": "A 啟用" },
            { "KEYCODE": "X", "NAME": "停用", "COMBITEM": "X 停用" }
        ]
    });

    // 查詢欄位
    var mLabelWidth = 90;
    var mWidth = 230;
    var T1Query = Ext.widget({
        xtype: 'form',
        layout: 'form',
        border: false,
        autoScroll: false, // 若為true,容器內容超出容器大小時出現捲動條;若為false超出容器的部分會被裁切掉
        fieldDefaults: {
            xtype: 'textfield',
            labelAlign: 'right',
            labelWidth: mLabelWidth,
            width: mWidth
        },
        items: [
            {
                xtype: 'panel',
                id: 'PanelP1',
                border: false,
                layout: 'hbox',
                items: [
                    {
                        xtype: 'combo',
                        fieldLabel: '庫房別',
                        name: 'WH_NO',
                        id: 'WH_NO',
                        store: st_wh_no,
                        queryMode: 'local',
                        displayField: 'COMBITEM',
                        valueField: 'VALUE',
                        multiSelect: false,
                        queryMode: 'local',
                        anyMatch: true,
                        autoSelect: true,
                        labelWidth:50,
                        width: 200,
                        padding: '0 4 0 4',
                        fieldCls: 'required',
                        tpl: '<tpl for="."><div class="x-boundlist-item" style="height:auto;">{COMBITEM}&nbsp;</div></tpl>', // 
                        listeners: {
                            beforequery: function (record) {
                                record.query = new RegExp(record.query, 'i');
                                record.forceAll = true;
                            },
                            select: function (combo, records, eOpts) {
                            }
                        }
                    }, {
                        xtype: 'combo',
                        fieldLabel: '物料類別',
                        name: 'MAT_CLASS',
                        id: 'MAT_CLASS',
                        store: st_matclass,
                        queryMode: 'local',
                        displayField: 'COMBITEM',
                        valueField: 'VALUE',
                        multiSelect: false,
                        queryMode: 'local',
                        anyMatch: true,
                        autoSelect: true,
                        labelWidth: 60,
                        width: 200,
                        padding: '0 4 0 4',
                        fieldCls: 'required',
                        tpl: '<tpl for="."><div class="x-boundlist-item" style="height:auto;">{COMBITEM}&nbsp;</div></tpl>', // 
                        listeners: {
                            beforequery: function (record) {
                                record.query = new RegExp(record.query, 'i');
                                record.forceAll = true;
                            },
                            select: function (combo, records, eOpts) {
                            }
                        }
                    }, {
                        xtype: 'monthfield',
                        fieldLabel: '年月',
                        name: 'DATA_YM_START',
                        id: 'DATA_YM_START',
                        labelWidth: 40,
                        width: 150,
                        value: getAddDate(-30),
                        padding: '0 4 0 4',
                        fieldCls: 'required'
                    }, {
                        xtype: 'monthfield',
                        fieldLabel: '至',
                        name: 'DATA_YM_END',
                        id: 'DATA_YM_END',
                        labelWidth: 10,
                        width: 120,
                        value: new Date(),
                        padding: '0 4 0 4',
                        fieldCls: 'required'
                    }                
                ]
            }, {
                xtype: 'panel',
                id: 'PanelP2',
                border: false,
                layout: 'hbox',
                padding: '8 4 8 4',
                items: [
                    {
                        xtype: 'radiogroup',
                        labelWidth: 25,
                        width: 350,
                        //anchor: '30%',
                        border: false,
                        padding: '0 4 0 4',
                        name: 'RADIO_BUTTON',
                        items: [
                            { boxLabel: '庫備品', width: 55, inputValue: 0, checked: true },
                            { boxLabel: '非庫備品(排除鎖E品項)', width: 150, inputValue: 1 },
                            { boxLabel: '庫備品(管控項目)', width: 130, inputValue: 2 }
                        ],
                        listeners:
                        {
                            beforequery: function (record) {
                                record.query = new RegExp(record.query, 'i');
                                record.forceAll = true;
                                //Ext.Msg.alert('說明', T1Query.getForm().findField('P0').getValue());
                            },
                            change: function (rg, nVal, oVal, eOpts) {

                            }
                        }
                    }, {
                        xtype: 'button',
                        text: '查詢',
                        iconCls: 'TRASearch',
                        handler: function () {
                            if (
                                T1Query.getForm().findField('WH_NO').getValue() != null &&      // 庫房別 
                                T1Query.getForm().findField('MAT_CLASS').getValue() != null &&  // 物料類別
                                T1Query.getForm().findField('DATA_YM_START').getValue() != null &&  // 年月
                                T1Query.getForm().findField('DATA_YM_END').getValue() != null &&  // 年月
                                true
                            ) {
                                Ext.ComponentQuery.query('panel[itemId=form]')[0].collapse();
                                T1Load();
                                msglabel('訊息區:');
                            }
                            else {
                                Ext.Msg.alert('提醒', '庫房別 與 物料類別 與 年月 必填');
                                msglabel('訊息區:庫房別 與 物料類別 與 年月 必填');
                            }
                        }
                    }, {
                        xtype: 'button',
                        text: '清除',
                        iconCls: 'TRAClear',
                        handler: function () {
                            var f = this.up('form').getForm();
                            f.reset();
                            f.findField('WH_NO').focus(); // 進入畫面時輸入游標預設在P0
                            f.findField('DATA_YM_START').setValue("");
                            f.findField('DATA_YM_END').setValue("");
                            msglabel('訊息區:');
                        }
                    }
                ]
            }
        ]
    });


    Ext.define('T1Model', {
        extend: 'Ext.data.Model',
        fields: [
            { name: 'WH_NO', type: 'string' }, // 00.庫房代碼
            { name: 'MMCODE', type: 'string' }, // 01.院內碼
            { name: 'APL_INQTY', type: 'string' }, // 02.入庫總量
            { name: 'DATA_YM', type: 'string' }, // 03.年月
            { name: 'MMNAME_C', type: 'string' }, // 04.中文品名
            { name: 'MMNAME_E', type: 'string' }, // 05.英文品名
            { name: 'MAT_CLASS', type: 'string' }, // 06.物料分類代碼
            { name: 'BASE_UNIT', type: 'string' }, // 07.計量單位代碼
            { name: 'M_STOREID', type: 'string' }, // 08.庫備識別碼
            { name: 'M_CONTID', type: 'string' }, // 09.合約識別碼
            { name: 'M_APPLYID', type: 'string' }, // 10.申請申購識別碼
            { name: 'DISC_UPRICE', type: 'string' }, // 11.優惠最小單價
            { name: 'WH_NAME', type: 'string' }, // 12.庫房名稱
            { name: 'WH_KIND', type: 'string' }, // 13.庫別分類
            { name: 'WH_GRADE', type: 'string' }, // 14.庫別級別
            { name: 'MAT_CLSNAME', type: 'string' }, // 15.物料分類名稱
            { name: 'MAT_CLSID', type: 'string' }, // 16.物料分類屬性
            { name: 'TOT', type: 'string' }, // 16.總價

        ]
    });
    //var T1Store = Ext.create('WEBAPP.store.AA.BG0002', { // 定義於/Scripts/app/store/PhVender.js
    //    listeners: {
    //        beforeload: function (store, options) {
    //            // 載入前將查詢條件P0~P4的值代入參數
    //            var np = {
    //                p0: T1Query.getForm().findField('P0').getValue(),
    //                p1: T1Query.getForm().findField('P1').getValue()
    //            };
    //            Ext.apply(store.proxy.extraParams, np);
    //        }
    //    }
    //});
    var T1Store = Ext.create('Ext.data.Store', {
        model: 'T1Model',
        pageSize: 20, // 每頁顯示筆數
        remoteSort: true,
        sorters: [{ property: 'MMCODE, data_ym', direction: 'DESC' }], // a.mmcode, b.data_ym
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/BG0002/GetAll',
            reader: {
                type: 'json',
                rootProperty: 'etts',
                totalProperty: 'rc'
            }
        }
        , listeners: {
            beforeload: function (store, options) {
                var np = {
                    WH_NO: T1Query.getForm().findField('WH_NO').getValue(), // 庫房別 
                    MAT_CLASS: T1Query.getForm().findField('MAT_CLASS').getValue(), // 物料類別
                    DATA_YM_START: T1Query.getForm().findField('DATA_YM_START').getValue(), // 年月開始 10801
                    DATA_YM_END: T1Query.getForm().findField('DATA_YM_END').getValue(), // 年月結束 10807
                    RADIO_BUTTON: T1Query.getForm().findField('RADIO_BUTTON').getChecked()[0].inputValue,
                    ENDL:''
                    //showdata: T1Query.getForm().findField('SHOWDATA').getChecked()[0].inputValue,
                };
                Ext.apply(store.proxy.extraParams, np);
            }
        }
    });
    
    function T1Load() {
        T1Tool.moveFirst();
    }

    // toolbar,包含換頁、新增/修改/刪除鈕
    var T1Tool = Ext.create('Ext.PagingToolbar', {
        store: T1Store,
        displayInfo: true,
        border: false,
        plain: true,
        buttons: [
            //{
            //    text: '新增', handler: function () {
            //        T1Set = '/api/BE0002/Create'; // BE0002Controller的Create
            //        msglabel('訊息區:');
            //        setFormT1('I', '新增');
            //    }
            //},
            //{
            //    itemId: 'edit', text: '修改', disabled: true, handler: function () {
            //        T1Set = '/api/BE0002/Update';
            //        msglabel('訊息區:');
            //        setFormT1("U", '修改');
            //    }
            //}
            //, {
            //    itemId: 'delete', text: '刪除', disabled: true,
            //    handler: function () {
            //        Ext.MessageBox.confirm('刪除', '是否確定刪除？', function (btn, text) {
            //            if (btn === 'yes') {
            //                T1Set = '/api/BE0002/Delete';
            //                T1Form.getForm().findField('x').setValue('D');
            //                T1Submit();
            //            }
            //        }
            //        );
            //    }
            //}
            {
                itemId: 'export', text: '匯出', disabled: false,
                handler: function () {
                    Ext.MessageBox.confirm('匯出', '是否確定匯出？', function (btn, text) {
                        if (btn === 'yes') {
                            var p = new Array();
                            p.push({ name: 'WH_NO', value: T1Query.getForm().findField('WH_NO').getValue() }); // 庫房別 
                            p.push({ name: 'MAT_CLASS', value: T1Query.getForm().findField('MAT_CLASS').getValue() }); //物料類別
                            p.push({ name: 'DATA_YM_START', value: gmtTimeToYmd(T1Query.getForm().findField('DATA_YM_START').getValue()) }); // 年月開始 10801
                            p.push({ name: 'DATA_YM_END', value: gmtTimeToYmd(T1Query.getForm().findField('DATA_YM_END').getValue()) }); // 年月結束 10801
                            p.push({ name: 'RADIO_BUTTON', value: T1Query.getForm().findField('RADIO_BUTTON').getChecked()[0].inputValue }); // 庫備種類
                            PostForm('/api/BG0002/Excel', p);
                        }
                    });
                }
            },
            {
                id: 't1print', text: '列印', disabled: false, handler: function () {
                    reportUrl = '/Report/B/BG0002.aspx';
                    showReport();
                }
            }
        ]
    });
    function setFormT1(x, t) {
        viewport.down('#t1Grid').mask();
        viewport.down('#form').setTitle(t + T1Name);
        viewport.down('#form').expand();
        var f = T1Form.getForm();
        //if (x === "I") {
        //    isNew = true;
        //    var r = Ext.create('WEBAPP.model.PhVender'); // /Scripts/app/model/PhVender.js
        //    T1Form.loadRecord(r); // 建立空白model,在新增時載入T1Form以清空欄位內容
        //    u = f.findField("AGEN_NO"); // 廠商碼在新增時才可填寫
        //    u.setReadOnly(false);
        //    u.clearInvalid();
        //    f.findField('REC_STATUS').setValue('A'); // 修改狀態碼預設為A
        //}
        //else {
        //    u = f.findField('AGEN_NAMEC');
        //}
        f.findField('x').setValue(x);

        //f.findField('AGEN_NAMEC').setReadOnly(false);
        //f.findField('AGEN_NAMEE').setReadOnly(false);
        //f.findField('AGEN_ADD').setReadOnly(false);
        //f.findField('AGEN_FAX').setReadOnly(false);
        //f.findField('AGEN_TEL').setReadOnly(false);
        //f.findField('AGEN_ACC').setReadOnly(false);
        //f.findField('UNI_NO').setReadOnly(false);
        //f.findField('AGEN_BOSS').setReadOnly(false);
        //f.findField('REC_STATUS').setReadOnly(false);
        //f.findField('EMAIL').setReadOnly(false);
        //f.findField('EMAIL_1').setReadOnly(false);
        //f.findField('AGEN_BANK').setReadOnly(false);
        //f.findField('AGEN_SUB').setReadOnly(false);
        T1Form.down('#cancel').setVisible(true);
        T1Form.down('#submit').setVisible(true);
        u.focus();
    }

    // 查詢結果資料列表
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
            text: "院內碼", dataIndex: 'MMCODE', width: 70, sortable: true
        }, {
            text: "英文品名", dataIndex: 'MMNAME_E', width: 200, sortable: true    
        }, {
            text: "中文品名", dataIndex: 'MMNAME_C', width: 200, sortable: true
        }, {
            text: "計量單位", dataIndex: 'BASE_UNIT', width: 80, sortable: true, align: 'right'
        }, {
            text: "單價", dataIndex: 'DISC_UPRICE', width: 60, sortable: true, align: 'right'
        }, {
            text: "年月", dataIndex: 'DATA_YM', width: 50, sortable: true
        }, {
            text: "本期增加", dataIndex: 'APL_INQTY', width: 90, sortable: true, align: 'right'
        }, {
             text: "總價", dataIndex: 'TOT', width: 60, sortable: true, align: 'right'
        }, {
            header: "",
            flex: 1
        }],
        viewConfig: {
            listeners: {
                refresh: function (view) {
                    T1Tool.down('#export').setDisabled(T1Store.getCount() === 0);
                    T1Tool.down('#t1print').setDisabled(T1Store.getCount() === 0);
                }
            }
        },
        listeners: {
            selectionchange: function (model, records) {
                T1Rec = records.length;
                T1LastRec = records[0];
                Ext.ComponentQuery.query('panel[itemId=form]')[0].expand();
                setFormT1a();
            }
        }
    });
    function setFormT1a() {
        // T1Grid.down('#edit').setDisabled(T1Rec === 0);
        // T1Grid.down('#delete').setDisabled(T1Rec === 0); // 若有刪除鈕,可在此控制是否可以按
        if (T1LastRec) {
            isNew = false;
            T1Form.loadRecord(T1LastRec);
            var f = T1Form.getForm();
            f.findField('x').setValue('U');
            //var u = f.findField('AGEN_NO');
            //u.setReadOnly(true);
            //u.setFieldStyle('border: 0px');
            //if (T1LastRec.data['REC_STATUS'] == 'X')
            //    T1Grid.down('#edit').setDisabled(true); // 停用的資料就不允許修改
        }
        else {
            T1Form.getForm().reset();
        }
    }

    // 顯示明細/新增/修改輸入欄
    var T1Form = Ext.widget({
        xtype: 'form',
        layout: 'form',
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
        items: [
            {
                fieldLabel: 'Update',
                name: 'x',
                xtype: 'hidden'
            }, {
                xtype: 'displayfield', fieldLabel: '院內碼', name: 'MMCODE'
            }, {
                xtype: 'displayfield', fieldLabel: '英文品名', name: 'MMNAME_E'
            }, {
                xtype: 'displayfield', fieldLabel: '中文品名', name: 'MMNAME_C'
            }, {
                xtype: 'displayfield', fieldLabel: '計量單位', name: 'BASE_UNIT'
            }, {
                xtype: 'displayfield', fieldLabel: '單價', name: 'DISC_UPRICE'
            }, {
                xtype: 'displayfield', fieldLabel: '年月', name: 'DATA_YM'
            }, {
                xtype: 'displayfield', fieldLabel: '本期增加', name: 'APL_INQTY'
            }, {
                xtype: 'displayfield', fieldLabel: '總價', name: 'TOT'
            }
        ],
        buttons: [{
            itemId: 'submit', text: '儲存', hidden: true,
            handler: function () {
                if (this.up('form').getForm().isValid()) { // 檢查T1Form填寫資料是否符合規則(必填欄位都有填、輸入內容有符合正規表示式等)
                    //if (this.up('form').getForm().findField('AGEN_NAMEC').getValue() == ''
                    //    && this.up('form').getForm().findField('AGEN_NAMEE').getValue() == '')
                    //    Ext.Msg.alert('提醒', '廠商中文名稱或廠商英文名稱至少需輸入一種');
                    //else
                    //{
                    //    var confirmSubmit = viewport.down('#form').title.substring(0, 2);
                    //    Ext.MessageBox.confirm(confirmSubmit, '是否確定' + confirmSubmit + '?', function (btn, text) {
                    //        if (btn === 'yes') {
                    //            T1Submit();
                    //        }
                    //    }
                    //    );
                    //}
                }
                else
                {
                    Ext.Msg.alert('提醒', '輸入資料格式有誤');
                    msglabel('訊息區:輸入資料格式有誤');
                }
            }
        }, {
            itemId: 'cancel', text: '取消', hidden: true, handler: T1Cleanup
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
                            // 新增後,將key代入查詢條件,只顯示剛新增的資料
                            var v = action.result.etts[0];
                            T1Query.getForm().findField('P0').setValue(v.AGEN_NO);
                            T1Load();
                            msglabel('訊息區:資料新增成功');
                            break;
                        case "U":
                            var v = action.result.etts[0];
                            r.set(v);
                            r.commit();
                            msglabel('訊息區:資料修改成功');
                            break;
                        //case "D":
                        //    T1Store.remove(r); // 若刪除後資料需從查詢結果移除可用remove
                        //    r.commit();
                        //    break;
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
        var f = T1Form.getForm();
        f.reset();
        f.getFields().each(function (fc) {
            if (fc.xtype == "displayfield" || fc.xtype == "textfield") {
                fc.setReadOnly(true);
            } else if (fc.xtype == "combo" || fc.xtype == "datefield") {
                fc.readOnly = true;
            }
        });
        T1Form.down('#cancel').hide();
        T1Form.down('#submit').hide();
        viewport.down('#form').setTitle('瀏覽');
        Ext.ComponentQuery.query('panel[itemId=form]')[0].collapse();
        setFormT1a();
    }


    var viewport = Ext.create('Ext.Viewport', {
        renderTo: body,
        layout: {
            type: 'border',
            padding: 0
        },
        defaults: {
            split: true
        },
        items: [{
            itemId: 't1Grid',
            region: 'center',
            layout: 'fit',
            collapsible: false,
            title: '',
            border: false,
            items: [T1Grid]
        },
        {
            itemId: 'form',
            region: 'east',
            collapsible: true,
            floatable: true,
            width: 300,
            title: '瀏覽',
            border: false,
            collapsed: true,
            layout: {
                type: 'fit',
                padding: 5,
                align: 'stretch'
            },
            items: [T1Form]
        }
        ]
    });

    //T1Load(); // 進入畫面時自動載入一次資料
    T1Query.getForm().findField('WH_NO').focus();

    // 載入預設資料
    //var comboWH_NO = T1Query.getForm().findField('WH_NO');
    //comboWH_NO.select(comboWH_NO.getStore().getAt(0));    // 若有資料combo選取第一筆資料
    //T1Query.getForm().findField('WH_NO').setValue(session['Inid']); // session['Inid'] + ' ' + session['InidName']; // 使用單位:庫房別 inid='560000 中央庫房'
    //T1Query.getForm().findField('MAT_CLASS').setValue('02');
    //var combo = T1Query.getForm().findField('P0');
    //combo.select(combo.getStore().getAt(0));    // 若有資料combo選取第一筆資料
});
