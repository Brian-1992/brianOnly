Ext.Loader.setConfig({
    enabled: true,
    paths: {
        'WEBAPP': '/Scripts/app'
    }
});

Ext.require(['WEBAPP.utils.Common']);

Ext.onReady(function () {
    var T1Name = "藥品採購申請作業";
    var mmPotSingleSet = '/api/BD0009/SetSingleMmPot';
    var mmPotMultiSet = '/api/BD0009/SetMultiMmPot';
    var tranSet = '/api/BD0009/SetTran';
    var whNoComboGet = '/api/BD0009/GetWH_NoCombo';
    var AgennoComboGet = '/api/BC0002/GetAgennoCombo';
    var totalCntPriceGet = '/api/BD0009/GetTotalCntPrice';
    var updateUpdateUser = '/api/BD0009/updateUpdateUser';
    var GetCalc = '/api/BD0009/GetCalc';
    var reportUrl = '/Report/B/BD0009.aspx';
    var ClearData = '/api/BD0009/ClearData';
    var T1Rec = 0;
    var T1LastRec = null;
    var reCalc = 'N'; // 是否為重算
    var reCalcDisRatio = 'N'; // 是否為優惠重算
    var keydownRec = null; // 最後一次按的方向鍵
    var maximunJumping = 100; // 遭遇不可編輯欄時最多可跳躍幾筆
    var jumpingTimes = 0; // 每次跳躍時使用的已跳計次
    var getLowFlag = 'N';
    var arrPM = ["005", "006", "007"];
    var currentIndex = -1;
    var editScrollY = 0;
    var isMouseClick = false;
    Ext.override(Ext.grid.column.Column, { menuDisabled: true });

    var whNoQueryStore = Ext.create('Ext.data.Store', {
        fields: ['VALUE', 'COMBITEM']
    });

    var lTypeStore = Ext.create('Ext.data.Store', {
        fields: ['statusName', 'statusId'],
        data: [
            { VALUE: '1', TEXT: '庫存量<=安全存量(安全存量>0 或 最低庫存量>0)' },
            { VALUE: '2', TEXT: '庫存量<=安全庫量(安全存量=0 且 最低庫存量=0)' },
            { VALUE: '3', TEXT: '庫存量>安全存量(安全存量>0 或 最低庫存量>0)' },
            { VALUE: '4', TEXT: '庫存量>安全存量(安全存量=0 且 最低庫存量=0)' },
            { VALUE: '5', TEXT: '全庫品項' }
        ]
    });
    var MMCODEStore = Ext.create('Ext.data.Store', {
        fields: ['VALUE', 'TEXT'],
        data: [
            { VALUE: '001', TEXT: '001' },
            { VALUE: '002', TEXT: '002' },
            { VALUE: '003', TEXT: '003' },
            { VALUE: '004', TEXT: '004' },
            { VALUE: '005', TEXT: '005' },
            { VALUE: '006', TEXT: '006' },
            { VALUE: '007', TEXT: '007' },
            { VALUE: '008', TEXT: '008' },
            { VALUE: '009', TEXT: '009' }
        ]
    });
    function setComboData() {
        Ext.Ajax.request({
            url: whNoComboGet,
            method: reqVal_p,
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    var tb_data = data.etts;
                    if (tb_data.length > 0) {
                        for (var i = 0; i < tb_data.length; i++) {
                            whNoQueryStore.add({ VALUE: tb_data[i].VALUE, COMBITEM: tb_data[i].COMBITEM });
                        }
                        T1Query.getForm().findField('P0').setValue(tb_data[0].VALUE);
                    }
                }
            },
            failure: function (response, options) {
                Ext.Msg.alert('訊息', '網頁已逾時，請重新登入。');
            }
        });
    }
    setComboData();
    function getCalcData() {
        Ext.Ajax.request({
            url: GetCalc,
            params: {
                purdate: T1Query.getForm().findField('P1').rawValue
            },
            method: reqVal_p,
            success: function (response) {
                var data = Ext.decode(response.responseText);
                T1Query.getForm().findField('P3').setValue(data.msg.toString());
            },
            failure: function (response, options) {
                Ext.Msg.alert('訊息', '網頁已逾時，請重新登入。');
            }
        });
    }
    var T1QueryAgenno = Ext.create('WEBAPP.form.AgenNoCombo', {
        name: 'P4',
        fieldLabel: '廠商代碼',
        limit: 20,
        queryUrl: AgennoComboGet,
        storeAutoLoad: true,
        insertEmptyRow: true,
        readOnly: false,
        listeners: {
            focus: function (field, event, eOpts) {
                if (!field.isExpanded) {
                    setTimeout(function () {
                        field.expand();
                    }, 300);
                }
            }
        }
    });

    var T1Query = Ext.widget({
        title: '查詢',
        itemId: 'form',
        xtype: 'form',
        layout: 'form',
        border: false,
        autoScroll: true,
        width: '100%',
        collapsible: true,
        hideCollapseTool: true,
        titleCollapse: true,
        fieldDefaults: {
            xtype: 'textfield',
            labelAlign: 'right',
            labelWidth: 80,
            width: 250
        },
        items: [{
            xtype: 'panel',
            id: 'PanelP1',
            border: false,
            layout: {
                type: 'box',
                vertical: false
            },
            items: [
                {
                    xtype: 'combo',
                    store: whNoQueryStore,
                    displayField: 'COMBITEM',
                    valueField: 'VALUE',
                    id: 'P0',
                    name: 'P0',
                    margin: '1 0 1 0',
                    fieldLabel: '庫別代碼',
                    fieldCls: 'required',
                    allowBlank: false,
                    queryMode: 'local',
                    autoSelect: true,
                    editable: true, typeAhead: true, forceSelection: true, selectOnFocus: true,
                    listeners: {
                        focus: function (field, event, eOpts) {
                            if (!field.isExpanded) {
                                setTimeout(function () {
                                    field.expand();
                                }, 300);
                            }
                        }
                    }
                }, {
                    xtype: 'datefield',
                    id: 'P1',
                    name: 'P1',
                    margin: '1 0 1 0',
                    fieldLabel: '採購日期',
                    fieldCls: 'required',
                    allowBlank: false,
                    listeners: {
                        focus: function (field, event, eOpts) {
                            if (!field.isExpanded) {
                                setTimeout(function () {
                                    field.expand();
                                }, 300);
                            }
                        },
                        change: function (field, newVal, oldVal) {
                            var getDate = new Date();

                            var today = (getDate.getFullYear() - 1911) + ("0" + (getDate.getMonth() + 1)).slice(-2) + ("0" + getDate.getDate()).slice(-2);
                            if (T1Query.getForm().findField('P1').rawValue == today) //
                            {
                                Ext.getCmp('reclc_id').setDisabled(false);
                                Ext.getCmp('reclc_disRatio').setDisabled(false);
                                T1Query.getForm().findField('P3').setDisabled(false);
                                T1Query.getForm().findField('P9').setDisabled(false);
                            }
                            else {
                                Ext.getCmp('reclc_id').setDisabled(true);
                                Ext.getCmp('reclc_disRatio').setDisabled(true);
                                T1Query.getForm().findField('P3').setValue('1');
                                T1Query.getForm().findField('P3').setDisabled(true);
                                T1Query.getForm().findField('P9').setValue(100);
                                T1Query.getForm().findField('P9').setDisabled(true);
                            }

                        }
                    }
                }, {
                    xtype: 'panel',
                    border: false,
                    margin: '1 0 1 0',
                    layout: {
                        type: 'hbox',
                        vertical: false
                    },
                    items: [{
                        xtype: 'button',
                        text: '查詢',
                        margin: '1',
                        handler: function () {
                            var f = this.up('form').getForm();
                            if (f.findField('P0').getValue() == null)
                                Ext.Msg.alert('提醒', '[庫別代碼]不可空白');
                            else if (!f.isValid())
                                Ext.Msg.alert('提醒', '查詢條件為必填');
                            else {
                                //Ext.ComponentQuery.query('panel[itemId=form]')[0].collapse();
                                reCalc = 'N';
                                T1Load();
                            }
                            msglabel('');
                        }
                    }, {
                        xtype: 'button',
                        text: '載入',
                        margin: '1',
                        handler: function () {
                            popItemImport();
                            msglabel('');
                        }
                    }, {
                        xtype: 'button',
                        text: '儲存',
                        margin: '1',
                        handler: function () {
                            Ext.MessageBox.confirm('訊息', '是否確定儲存?', function (btn, text) {
                                if (btn === 'yes') {
                                    //T1Submit();
                                    updateUpUser();
                                    T1Grid.getSelectionModel().deselectAll();
                                }
                            });
                        }
                    }, {
                        xtype: 'button',
                        text: '刪除',
                        margin: '1',
                        handler: function () {
                            T1Grid.getSelectionModel()
                            Ext.MessageBox.confirm('訊息', '確定刪除訂單資料?', function (btn, text) {
                                if (btn === 'yes') {
                                    DeleteRec();
                                }
                            });
                        }
                    }, {
                        xtype: 'button',
                        text: '清空',
                        margin: '1',
                        handler: function () {
                            Ext.MessageBox.confirm('訊息', '是否確定清空日期<font color=red>' + T1Query.getForm().findField('P1').rawValue + '</font>尚未轉出訂單資料?', function (btn, text) {
                                if (btn === 'yes') {
                                    DoClearData();
                                    T1Grid.getSelectionModel().deselectAll();
                                }
                            });
                        }
                    }
                    ]
                    //}, {
                    //    xtype: 'radiogroup',
                    //    id: 'TRTYP_GRP',
                    //    name: 'TRTYP_GRP',
                    //    anchor: '35%',
                    //    width: 300,
                    //    items: [
                    //        { boxLabel: '本月訂單', width: '50%', name: 'TRTYP', inputValue: 1, checked: true },
                    //        { boxLabel: '下月訂單', width: '50%', name: 'TRTYP', inputValue: 2 }
                    //    ]
                }
            ]
        }, {
            xtype: 'panel',
            id: 'PanelP2',
            border: false,
            layout: {
                type: 'box',
                vertical: false
            },
            items: [
                {
                    xtype: 'panel',
                    border: false,
                    margin: '1 0 1 0',
                    layout: {
                        type: 'hbox',
                        vertical: false
                    },
                    width: 250,
                    items: [
                        {
                            xtype: 'textfield',
                            fieldLabel: '實際採購量x',
                            name: 'P3',
                            id: 'P3',
                            margin: '1 0 1 0',
                            labelWidth: 80,
                            width: '80%',
                            labelSeparator: '',
                            maskRe: /[0-9.]/,
                            regexText: '請輸入最多小數兩位的數字',
                            regex: /^(([1-9][0-9]*|0)(\.[0-9]{1,2})?)$/
                        }, {
                            xtype: 'button',
                            id: 'reclc_id',
                            text: '重算',
                            margin: '1',
                            handler: function () {
                                // 2019/10/08重算由原本重算前端後整批資料submit更新資料庫,改為用call SP重算資料後,reload
                                // reCultQty();
                                reCalc = 'Y';
                                T1Load();
                            }
                        }
                    ]
                }, T1QueryAgenno, {
                    xtype: 'textfield',
                    fieldLabel: '院內碼',
                    name: 'P8',
                    id: 'P8',
                    margin: '1 0 1 0'
                }
                //, {
                //    xtype: 'checkbox',
                //    boxLabel: '本日出帳',
                //    padding: '0 0 0 2',
                //    labelAlign: 'right',
                //    labelSeparator: '',
                //    name: 'P5',
                //    id: 'P5',
                //    plugins: 'responsive',
                //    responsiveConfig: {
                //        'width < 600': {
                //            width: '30%'
                //        },
                //        'width >= 600': {
                //            width: '6%'
                //        }
                //    }
                //}
                , {
                    xtype: 'checkbox',
                    boxLabel: '申購中',
                    padding: '0 0 0 2',
                    labelAlign: 'right',
                    labelSeparator: '',
                    name: 'P7',
                    id: 'P7',
                    width: 100
                }, {
                    xtype: 'checkbox',
                    boxLabel: '已轉出訂單',
                    padding: '0 0 0 2',
                    labelAlign: 'right',
                    labelSeparator: '',
                    name: 'P6',
                    id: 'P6',
                    width: 130
                }
            ]
        }, {
            xtype: 'panel',
            id: 'PanelP4',
            border: false,
            layout: {
                type: 'box',
                vertical: false
            },
            width: 300,
            items: [
                {
                    xtype: 'numberfield',
                    fieldLabel: '單次採購優惠數量x',
                    name: 'P9',
                    id: 'P9',
                    margin: '1 0 1 0',
                    labelWidth: 110,
                    width: 150,
                    labelSeparator: '',
                    value: 100,
                    minValue: 1,
                    hideTrigger: true,
                    decimalPrecision:0,
              }, {
                        xtype: 'displayfield',
                        //fieldLabel: '%',
                        value: '%',
                        width: 15,
                        //labelSeparator: '',
                        margin: '1 0 1 0'
                    }, {
                        xtype: 'button',
                        id: 'reclc_disRatio',
                        text: '優惠重算',
                        margin: '1',
                        handler: function () {
                            // 2019/10/08重算由原本重算前端後整批資料submit更新資料庫,改為用call SP重算資料後,reload
                            // reCultQty();
                            reCalcDisRatio = 'Y';
                            T1Load();
                        }
                    }
                ]
            }, {
            xtype: 'panel',
            id: 'PanelP3',
            border: false,
            layout: {
                type: 'box',
                vertical: false
            },
            items: [

                {
                    xtype: 'displayfield',
                    fieldLabel: '採購總項次',
                    name: 'D1',
                    id: 'D1',
                    margin: '1 0 1 0'
                }, {
                    xtype: 'displayfield',
                    fieldLabel: '採購總金額',
                    name: 'D2',
                    id: 'D2',
                    margin: '1 0 1 0'
                }, {
                    xtype: 'panel',
                    border: false,
                    margin: '1 0 1 0',
                    layout: {
                        type: 'hbox',
                        vertical: false
                    },
                    items: [{
                        xtype: 'button',
                        text: '轉出訂單',
                        margin: '1',
                        handler: function () {
                            //取消本月訂單 下月訂單 區分
                            //var trtyp = T1Query.getForm().findField('TRTYP_GRP').getValue()['TRTYP'];
                            //var extraMsg = '';
                            //if (trtyp == '1')
                            //    extraMsg = ' 本月訂單';
                            //else if (trtyp == '2')
                            //    extraMsg = ' 下月訂單';

                            //Ext.MessageBox.confirm('訊息', '要將勾選品項轉出' + extraMsg + '?', function (btn, text) {
                            //    if (btn === 'yes') {
                            //        if (trtyp == '1')
                            //            T1Tran('T');
                            //        else if (trtyp == '2')
                            //            T1Tran('N');
                            //    }
                            //});
                            T1Tran('T');
                        }
                    }, {
                        xtype: 'button',
                        text: '申購報表',
                        margin: '1',
                        handler: function () {
                            showReport();
                        }
                    }]
                }
            ]
        }]
    });

    var T1Store = Ext.create('WEBAPP.store.BD0009', {
        listeners: {
            beforeload: function (store, options) {
                var np = {
                    p0: T1Query.getForm().findField('P0').getValue(),
                    p1: T1Query.getForm().findField('P1').getValue(),
                    p1_1: T1Query.getForm().findField('P1').rawValue,
                    p2: '1', //T1Query.getForm().findField('P2').getValue(),  //移到載入Form T2Query
                    p4: T1Query.getForm().findField('P4').getValue(),
                    // p5: T1Query.getForm().findField('P5').getValue(),
                    p6: T1Query.getForm().findField('P6').getValue(),
                    p7: T1Query.getForm().findField('P7').getValue(),
                    p8: T1Query.getForm().findField('P8').getValue(),
                    recalc: reCalc,
                    icalc: T1Query.getForm().findField('P3').getValue(),
                    disRatio: T1Query.getForm().findField('P9').getValue(),
                    getLowFlag: getLowFlag,
                    reCalcDisRatio: reCalcDisRatio
                };
                Ext.apply(store.proxy.extraParams, np);
            },
            load: function (store, records, successful, eOpts) {
                if (!successful) {
                    T1Store.removeAll();
                    T1Tool.onLoad();
                }
                else {
                    //selectIsToday();
                    setTotalCntPrice();
                }
                getLowFlag = 'N';  // 第2次之後就不載入低於安全存量
                reCalc = 'N'; // 每次查完設為'N'
                reCalcDisRatio = 'N'; // 每次查完設為'N'

            }
        }
    });

    Ext.define('T2Model', {
        extend: 'Ext.data.Model',
        fields: ['E_VACCINE', 'E_RESTRICTCODE', 'MMCODE', 'MMNAME_E', 'INV_QTY', 'OPER_QTY,', 'SAFE_QTY', 'ADVISEQTY', 'CONTRACNO', 'APL_INVQTY', 'APL_OUTQTY', 'TODAYFLAG', 'E_PURTYPE', 'AGEN_NO', 'E_PURTYPE', 'CONTRACNO', 'AGEN_NO', 'PO_PRICE', 'M_DISCPERC', 'UNIT_SWAP', 'PACK_QTY0', 'M_PURUN', 'DISC_CPRICE']
    });

    function T1Load() {
        T1Tool.moveFirst();
    }

    var T1Tool = Ext.create('Ext.PagingToolbar', {
        store: T1Store,
        displayInfo: true,
        border: false,
        plain: true
    });
    var checkboxT1Model = Ext.create('Ext.selection.CheckboxModel', {
        checkOnly: true,
        //checkSelector: '.' + Ext.baseCSSPrefix,
        injectCheckbox: 'first',
        mode: 'SIMPLE',
        listeners: {
            beforeselect: function (model, record, index, eOpts) {
                if (record.data['ISTRAN'] != 'N' && record.data['ISTRAN'] != '')
                    return false;
            }
        }
    });

    function cellShifting(editP, vShift, hShift) {
        isMouseClick = false;
        var dataUpdated = false; // 本次做編輯欄位平移時,是否有更新資料
        var xScroll = T1Grid.getScrollX();
        var yScroll = T1Grid.getScrollY();
        var myMask = new Ext.LoadMask(viewport, { msg: '處理中...' });

        editP.completeEdit();

        var modifiedRec = T1Store.getModifiedRecords();
        if (modifiedRec.length > 0) // 有異動才更新store
        {
            // 計算金額
            var index = T1Store.indexOf(editP.context.record);
            var tmpPO_AMT = Math.round(parseFloat(editP.context.record.data['PO_QTY']) * parseFloat(editP.context.record.data['DISC_CPRICE']));
            if (!tmpPO_AMT > 0)
                tmpPO_AMT = 0;
            editP.context.record.data['PO_AMT'] = tmpPO_AMT;
            T1Store.data.items[index].data.PO_AMT = editP.context.record.data['PO_AMT'];
            if (parseFloat(editP.context.record.data['PO_QTY']) > 0)
                T1Store.data.items[index].data.ISTRAN_1 = '申購';

            dataUpdated = true;
            myMask.show();
            // setStore時會導致目前編輯的資料一律捲至畫面最下方,造成閱讀困難
            // 故增加以下處理:
            // 1.先記錄目前捲動條的xScroll和yScroll,在setStore後一段延遲後,畫面捲回記錄的位置
            // 2.此時避免畫面先捲到資料置底再捲回原位置造成觀看不直觀,處理期間先將畫面mask起來
            // 3.避免每次移動編輯欄都要mask畫面造成操作不便,改為判斷若本次資料有異動,才做setStore和延遲捲動
            T1Grid.setStore(T1Store);
        }
        var skip = 0;

        if (vShift > 0) {
            skip = 1;
            for (var i = editP.context.rowIdx + 1; i < T1Store.getData().length; i++) {
                var temp = T1Store.getAt(i);
                if (temp.data['ISTRAN'] != 'N' && temp.data['ISTRAN'] != '') {
                    skip++;
                } else {
                    break;
                }
            }
        }
        if (vShift < 0) {
            skip = -1;
            for (var j = editP.context.rowIdx - 1; j > -1; j--) {
                var temp = T1Store.getAt(j);
                if (temp.data['ISTRAN'] != 'N' && temp.data['ISTRAN'] != '') {
                    skip--;
                } else {
                    break;
                }
            }
        }

        // 狀態不是申購則不可編輯
        if (editP.context.record.data['ISTRAN'] != 'N' && editP.context.record.data['ISTRAN'] != '') {
            //Ext.Msg.alert('訊息', '院內碼' + editP.context.record.data['MMCODE'] + '狀態不是申購');
            sm.deselect(editP.context.rowIdx + vShift);
            editP.completeEdit();
        }

        //// 在每次完成編輯後scroll一下,以避免偶爾會出現Grid標頭與資料沒有對齊的情形
        //setTimeout(function () {
        //    T1Grid.scrollBy(0, 1, true);
        //}, 100);

        var timeDelay = 0;
        if (dataUpdated)
            timeDelay = 1000; // 若資料有更新,則延遲後再做scroll和startEdit,否則直接做
        setTimeout(function () {
            //var yShifted = 0;
            //if (vShift > 0)
            //    yShifted = 21; // 一列捲動大約21
            //else if (vShift < 0)
            //    yShifted = -21;
            //var yScrolling = yScroll + yShifted;
            //if (yScrolling < 0)
            //    yScrolling = 0;
            //T1Grid.scrollTo(xScroll, yScrolling, false);
            
            
            
            //T1Grid.getView().scrollRowIntoView(currentIndex + skip);

            // hShift採用位移的方式會有自訂欄位排列後順序不一樣的問題
            // 需取欄位目前index
            // hShift = 1 : 移到備註; hShift = -1 : 移到實際採購量
            var tColIdx = editP.context.colIdx;
            if (hShift > 0)
                tColIdx = T1Grid.down('#COL_MEMO').getIndex();
            else if (hShift < 0)
                tColIdx = T1Grid.down('#COL_PO_QTY').getIndex();
            var sm = T1Grid.getSelectionModel();
            sm.deselectAll();
            // sm.select(editP.context.rowIdx + vShift);
            
            if (editP.context.rowIdx + skip <= T1Store.getData().length - 1 && editP.context.rowIdx + skip >= 0) {
                editP.startEdit(editP.context.rowIdx + skip, tColIdx); // 編輯指定Idx的欄位
            } else {
                editP.startEdit(editP.context.rowIdx, tColIdx); // 編輯指定Idx的欄位
            }
        
            // statrEdit會觸發beforeedit,若指定欄位不可編輯,在beforeedit重新指向新的欄位

            if (dataUpdated) {
                myMask.hide();
                dataUpdated = false;
            }

            var yScrolling = yScroll + (20 * skip);
            //T1Grid.scrollTo(xScroll, yScrolling, false);

        }, timeDelay);
    }

    var T1Grid = Ext.create('Ext.grid.Panel', {
        //title: T1Name,
        store: T1Store,
        plain: true,
        loadingText: '處理中...',
        loadMask: true,
        markDirty: false,
        preserveScrollOnRefresh: true,
        //plugins: 'bufferedrenderer',
        cls: 'T1',
        dockedItems: [{
            dock: 'top',
            xtype: 'toolbar',
            items: [T1Query]
        }],
        selModel: checkboxT1Model,
        selType: 'checkboxmodel',
        columns: {
            defaults: {
                plugins: 'responsive',
                responsiveConfig: {
                    'width < 600': {
                        hidden: true
                    },
                    'width >= 600': {
                        hidden: false
                    }
                }
            },
            items: [{
                xtype: 'rownumberer',
                width: 40
            },
            //{
            //    text: "本日出帳",
            //    dataIndex: 'TODAYFLAG',
            //    align: 'center',
            //    style: 'text-align:left',
            //    width: 40,
            //    plugins: ''
            //},
            {
                text: "新增",
                dataIndex: 'NEWFLAG',
                width: 30,
                plugins: ''
            }, {
                text: "異動",
                dataIndex: 'CHGFLAG',
                width: 40,
                plugins: ''
            }, {
                text: "院內碼",
                dataIndex: 'MMCODE',
                width: 70,
                plugins: ''
            }, {
                text: "藥品名稱",
                dataIndex: 'MMNAME_E',
                width: 150,
                plugins: ''
            }, {
                text: "庫存量",
                dataIndex: 'INV_QTY',
                align: 'right',
                style: 'text-align:right',
                width: 60
            }, {
                text: "全院存量",
                dataIndex: 'ALLQTY',
                align: 'right',
                style: 'text-align:right',
                width: 70
            }, {
                text: "建議採購量",
                dataIndex: 'ADVISEQTY',
                align: 'right',
                style: 'text-align:right',
                width: 80,
                renderer: function (val, meta, record) {
                    if (record.data['E_PURTYPE'] == '2' && parseFloat(record.data['LOW_QTY']) > 0)
                        return '<font color="blue"><b>' + val + '</b></font>';
                    else
                        return val;
                }
            },
            {
                text: "<font color='red'>實際採購量</font>",
                itemId: 'COL_PO_QTY',
                dataIndex: 'PO_QTY',
                align: 'right',
                style: 'text-align:right',
                width: 80,
                plugins: '',
                renderer: function (val, meta, record) {
                    if (record.data['E_PURTYPE'] == '1' && (parseFloat(record.data['APL_INQTY']) >= parseFloat(record.data['OPER_QTY'])))
                        meta.style = "color:red;font-weight:bold;";
                    else if (record.data['E_PURTYPE'] == '2' && (parseInt(record.data['APL_INQTY']) >= parseInt(record.data['OPER_QTY']) * 1.5))
                        meta.style = "color:red;font-weight:bold;";
                    return val;
                },
                editor: {
                    xtype: 'textfield',
                    maskRe: /[0-9]/,
                    regexText: '只能輸入數字',
                    regex: /^([1-9][0-9]*|0)$/,
                    selectOnFocus: true,
                    listeners: {
                        activate: function (field, eOpts) {
                            
                        },
                        change: function (field, newVal, oldVal) {
                            //if (T1LastRec.data['ISTRAN'] != 'N' && T1LastRec.data['ISTRAN'] != '') {
                            //    // ISTRAN不是N則不可選取
                            //    T1Grid.getSelectionModel().deselect(T1LastRec, true, true);
                            //}
                            //else {
                            //    if (newVal != field.originalValue) {
                            //        T1Grid.getSelectionModel().select(T1LastRec, true, true);
                            //    }
                            //    else {
                            //        T1Grid.getSelectionModel().deselect(T1LastRec, true, true);
                            //    }
                            //    T1LastRec.data['PO_QTY'] = newVal;
                            //}
                        },
                        blur: function (field, event, eOpts) {
                            // 計算[金額]
                            //if (T1LastRec.data['ISTRAN'] == 'N' || T1LastRec.data['ISTRAN'] == '') {
                            //    var index = T1Store.indexOf(T1LastRec);
                            //    var tmpPO_AMT = parseInt(parseFloat(T1LastRec.data['PO_QTY']) * parseFloat(T1LastRec.data['DISC_CPRICE']));
                            //    if (!tmpPO_AMT > 0)
                            //        tmpPO_AMT = 0;
                            //    T1LastRec.data['PO_AMT'] = tmpPO_AMT;
                            //    T1Store.data.items[index].data.PO_AMT = T1LastRec.data['PO_AMT'];
                            //    T1Grid.setStore(T1Store);
                            //}
                        },
                        specialkey: function (field, e) {
                            
                            if (e.getKey() === e.UP) {
                                var editPlugin = this.up().editingPlugin;
                                keydownRec = 'U';
                                jumpingTimes = maximunJumping;
                                cellShifting(editPlugin, -1, 0);
                            }
                            else if (e.getKey() === e.DOWN || e.getKey() === e.ENTER) {
                                var editPlugin = this.up().editingPlugin;
                                
                                keydownRec = 'D';
                                jumpingTimes = maximunJumping;
                                cellShifting(editPlugin, 1, 0);
                            }
                            else if (e.getKey() === e.RIGHT) {
                                var editPlugin = this.up().editingPlugin;
                                keydownRec = 'R';
                                jumpingTimes = maximunJumping;
                                cellShifting(editPlugin, 0, 1);
                            }
                            else if (e.getKey() === e.LEFT) {
                                var editPlugin = this.up().editingPlugin;
                                keydownRec = 'L';
                                jumpingTimes = maximunJumping;
                                cellShifting(editPlugin, 0, 1);
                            }
                        }
                    }

                }
            },

            {
                text: "單次採購優惠數量",
                dataIndex: 'DISCOUNT_QTY',
                width: 80,
            },
            {
                text: "單次訂購達優惠數量成本價",
                dataIndex: 'DISC_COST_UPRICE',
                width: 80,
            }, {
                text: "備註",
                itemId: 'COL_MEMO',
                dataIndex: 'MEMO',
                width: 70,
                editor: {
                    xtype: 'textfield',
                    enforceMaxLength: true,
                    maxLength: 300,
                    listeners: {
                        change: function (field, newVal, oldVal) {

                        },
                        blur: function (field, event, eOpts) {

                        },
                        specialkey: function (field, e) {
                            if (e.getKey() === e.UP) {
                                var editPlugin = this.up().editingPlugin;
                                keydownRec = 'U';
                                jumpingTimes = maximunJumping;
                                cellShifting(editPlugin, -1, 0);
                            }
                            else if (e.getKey() === e.DOWN || e.getKey() === e.ENTER) {
                                var editPlugin = this.up().editingPlugin;
                                keydownRec = 'D';
                                jumpingTimes = maximunJumping;
                                cellShifting(editPlugin, 1, 0);
                            }
                            else if (e.getKey() === e.RIGHT) {
                                var editPlugin = this.up().editingPlugin;
                                keydownRec = 'R';
                                jumpingTimes = maximunJumping;
                                cellShifting(editPlugin, 0, -1);
                            }
                            else if (e.getKey() === e.LEFT) {
                                var editPlugin = this.up().editingPlugin;
                                keydownRec = 'L';
                                jumpingTimes = maximunJumping;
                                cellShifting(editPlugin, 0, -1);
                            }
                        }
                    }

                }
            }, {
                text: "最小包裝量",
                dataIndex: 'MIN_ORDQTY',
                align: 'right',
                style: 'text-align:center',
                width: 60,
                plugins: ''
            }, {
                text: "進價",
                align: 'right',
                style: 'text-align:center',
                dataIndex: 'PO_PRICE',
                width: 80
            }, {
                text: "金額",
                align: 'right',
                style: 'text-align:center',
                dataIndex: 'PO_AMT',
                width: 100
            }, {
                text: "廠商代碼",
                dataIndex: 'AGEN_NAME',
                width: 100
                //}, {
                //    text: "原裝箱數量",
                //    dataIndex: 'PACK_QTY0',
                //    align: 'right',
                //    style: 'text-align:left',
                //    width: 80
            }, {
                text: "最低存量",
                dataIndex: 'LOW_QTY',
                align: 'right',
                style: 'text-align:center',
                width: 60,
                plugins: ''
            }, {
                text: "合約碼",
                dataIndex: 'CONTRACNO',
                width: 50
            }, {
                text: "案別",
                dataIndex: 'PURTYPE',
                width: 50
            }, {
                text: "作業量",
                dataIndex: 'OPER_QTY',
                align: 'right',
                style: 'text-align:center',
                width: 60,
                plugins: ''
            }, {
                text: "安全存量",
                dataIndex: 'SAFE_QTY',
                align: 'right',
                style: 'text-align:center',
                width: 60,
                plugins: ''
            }, {
                text: "進貨量(本月累計)",
                align: 'right',
                style: 'text-align:center',
                dataIndex: 'APL_INQTY',
                width: 60
            }, {
                text: "消耗量(本月全院累計)",
                align: 'right',
                style: 'text-align:center',
                dataIndex: 'APL_OUTQTY',
                width: 60
            }, {
                text: "醫令扣庫(本月)",
                align: 'right',
                style: 'text-align:center',
                dataIndex: 'USEQTY',
                width: 60
                //}, {
                //    text: "退藥量(本月)",
                //    align: 'right',
                //    style: 'text-align:center',
                //    dataIndex: 'BACKQTY',
                //    width: 60
            }, {
                text: "零購本月預估量",
                align: 'right',
                style: 'text-align:center',
                dataIndex: 'ESTQTY',
                width: 60
            }, {
                text: "轉訂單",
                dataIndex: 'ISTRAN_1',
                width: 80
            }, {
                text: "疫苗",
                dataIndex: 'E_VACCINE',
                width: 40,
                plugins: ''
            }, {
                text: "管藥",
                dataIndex: 'E_RESTRICTCODE',
                width: 40,
                plugins: ''
            }, {
                text: "單次訂購達優惠數量折讓意願",
                dataIndex: 'ISWILLING',
                width: 80,
            }
                //{
                //    text: "轉換率",
                //    dataIndex: 'UNIT_SWAP',
                //    align: 'right',
                //    style: 'text-align:left',
                //    width: 60
                //},
                , {
                text: "包裝單位",
                dataIndex: 'M_PURUN',
                width: 70
            }, {
                text: "製造商",
                dataIndex: 'E_MANUFACT',
                width: 100
            }, {
                text: "重算",
                dataIndex: 'CALC',
                width: 80
            }, {
                text: "建立時間",
                dataIndex: 'CREATE_TIME',
                width: 100
            }, {
                text: "處理時間",
                dataIndex: 'UPDATE_TIME',
                width: 100
            }, {
                text: " ",
                align: 'left',
                flex: 1
            }]
        },
        plugins: [
            Ext.create('Ext.grid.plugin.CellEditing', {
                clicksToEdit: 1,//控制點擊幾下啟動編輯
                listeners: {
                    beforeedit: function (editor, context, eOpts) {
                        editScrollY = T1Grid.getScrollY();
                        currentIndex = context.rowIdx;
                        console.log('beforeedit plugin:' + currentIndex);
                    },
                    edit: function (editor, context, eOpts) {
                        console.log('edit plugin:' + currentIndex);
                        editScrollY = T1Grid.getScrollY();
                        var modifiedRec = context.store.getModifiedRecords();
                        var r = context.record;
                        currentIndex = T1Store.indexOfId(r.id);
                        if (modifiedRec.length > 0) // 有異動才request
                        {
                            // ============重算金額並setStore的部分若放在request的success才做,會導致編輯模式editor已清空的資料被恢復
                            setTotalCntPrice();

                            // 計算金額
                            var index = T1Store.indexOf(r);
                            var tmpPO_AMT = Math.round(parseFloat(r.get('PO_QTY')) * parseFloat(r.get('DISC_CPRICE')));
                            if (!tmpPO_AMT > 0)
                                tmpPO_AMT = 0;
                            T1Store.data.items[index].data.PO_AMT = tmpPO_AMT;
                            T1Grid.setStore(T1Store);
                            // ============

                            Ext.Ajax.request({
                                url: mmPotSingleSet,
                                params: {
                                    purdate: T1Query.getForm().findField('P1').getValue(),
                                    mmcode: r.get('MMCODE'),
                                    wh_no: r.get('WH_NO'),
                                    contracno: r.get('CONTRACNO'),
                                    agen_no: r.get('AGEN_NO'),
                                    istran: r.get('ISTRAN'),
                                    po_qty: r.get('PO_QTY'),
                                    po_price: r.get('PO_PRICE'),
                                    disc_cprice: r.get('DISC_CPRICE'),
                                    m_purun: r.get('M_PURUN'),
                                    po_amt: r.get('PO_AMT').toString().replace(/,/g, ''),
                                    m_discperc: r.get('M_DISCPERC'),
                                    memo: r.get('MEMO'),
                                    unit_swap: r.get('UNIT_SWAP'),
                                    adviseqty: r.get('ADVISEQTY'),
                                    e_purtype: r.get('E_PURTYPE'),
                                    flag: r.get('FLAG'),
                                    safe_qty: r.get('SAFE_QTY'),
                                    oper_qty: r.get('OPER_QTY'),
                                    low_qty: r.get('LOW_QTY'),
                                    min_ordqty: r.get('MIN_ORDQTY'),
                                    inv_qty: r.get('INV_QTY'),
                                    allqty: r.get('ALLQTY'),
                                    calc: r.get('CALC')
                                },
                                method: reqVal_p,
                                success: function (response) {
                                    msglabel('院內碼' + r.get('MMCODE') + '儲存完成');
                                    context.store.commitChanges(); // commit以移除紅色三角形標記,並讓getModifiedRecords能確實取到每次異動資料筆數
                                    //T1Load();
                                    var xScroll = T1Grid.getScrollX();
                                    var yScroll = T1Grid.getScrollY();
                                    var columnName = '#COL_' + context.field;
                                    

                                    var record = T1Store.getAt(currentIndex);

                                    
                                    var tColIdx = T1Grid.down(columnName).getIndex();
                                    T1Grid.scrollTo(xScroll, editScrollY, false);
                                    
                                    //T1Grid.editingPlugin.startEdit(currentIndex, tColIdx);
                                },
                                failure: function (response, options) {
                                    Ext.Msg.alert('訊息', '網頁已逾時，請重新登入。');
                                }
                            });
                            
                            //T1Grid.scrollTo(0, currentIndex * 21, false);
                        }
                    }
                }
            })
        ],
        listeners: {
            itemclick: function (self, record, item, index, e, eOpts) {
                T1LastRec = record;
                currentIndex = index;
                console.log('itemclick:' + currentIndex);
            },
            beforeedit: function (editor, e) {
                console.log('beforeedit grid:' + currentIndex);
                if (e.record.get('ISTRAN') != 'N' && e.record.get('ISTRAN') != '') {

                    return false; // ISTRAN不是N或空白則不可填寫, 跳至下一筆開始edit
                }

                // 實際採購量開始編輯時先清除舊值,備註開始編輯時不清空,以能夠改回空白
                // 2019/12/10要求改為保留舊值但focus時選取原資料
                //if (e.colIdx == T1Grid.down('#COL_PO_QTY').getIndex())
                //    e.value = '';
            },
            cellClick: function (self, td, cellIndex, record, tr, rowIndex, e, eOpts) {
                var columns = T1Grid.getColumns();
                var index = getColumnIndex(columns, 'CHK_NO');

                console.log('cellclick:' + rowIndex);
            }
        }
    });

    function getColumnIndex(columns, dataIndex) {
        var index = -1;
        for (var i = 0; i < columns.length; i++) {
            if (columns[i].dataIndex == dataIndex) {
                index = i;
            }
        }

        return index;
    }

    // 依實際採購量倍率及建議採購量重新計算實際採購量(PO_QTY)及金額(PO_AMT)
    //function reCultQty() {
    //    if (T1Query.getForm().findField('P3').getValue()) {
    //        if (T1Query.getForm().findField('P3').isValid()) {
    //            for (var i = 0; i < T1Store.data.items.length; i++) {
    //                T1Store.data.items[i].data.PO_QTY = Math.ceil(parseFloat(T1Store.data.items[i].data.ADVISEQTY) * parseFloat(T1Query.getForm().findField('P3').getValue()));
    //                T1Store.data.items[i].data.PO_AMT = Math.round((T1Store.data.items[i].data.PO_QTY * parseFloat(T1Store.data.items[i].data.DISC_CPRICE)) * 100) / 100;

    //            }
    //            T1Grid.setStore(T1Store);
    //            T1Grid.getSelectionModel().selectAll();
    //            msglabel('重算完成');
    //            T1Submit();
    //        }
    //        else
    //            Ext.Msg.alert('訊息', '[實際採購量x]輸入格式錯誤');
    //    }
    //    else
    //        Ext.Msg.alert('訊息', '[實際採購量x]需填寫');
    //}

    //function T1Submit() {
    //    var records = T1Grid.getSelection();
    //    var submitS = ''; // 單筆資料
    //    var submitT = ''; // 全部資料
    //    if (records.length > 0) {
    //        for (var i = 0; i < records.length; i++) {
    //            submitS = records[i].get('MMCODE') + "^" + records[i].get('WH_NO') + "^" + records[i].get('CONTRACNO') + "^" + records[i].get('AGEN_NO')
    //                + "^" + records[i].get('ISTRAN') + "^" + records[i].get('PO_QTY') + "^" + records[i].get('DISC_CPRICE')
    //                + "^" + records[i].get('M_PURUN') + "^" + records[i].get('PO_AMT') + "^" + records[i].get('M_DISCPERC') + "^" + records[i].get('MEMO')
    //                + "^" + records[i].get('UNIT_SWAP') + "^" + records[i].get('ADVISEQTY') + "^" + records[i].get('E_PURTYPE') + "^" + records[i].get('FLAG');
    //            if (i == records.length - 1)
    //                submitT += submitS;
    //            else
    //                submitT += submitS + "ˋ";
    //        }
    //        Ext.Ajax.request({
    //            url: mmPotMultiSet,
    //            params: {
    //                purdate: T1Query.getForm().findField('P1').getValue(),
    //                submitt: submitT
    //            },
    //            method: reqVal_p,
    //            success: function (response) {
    //                T1Grid.getSelectionModel().deselectAll();
    //                T1Load();
    //                msglabel('儲存完成');
    //            },
    //            failure: function (response, options) {

    //            }
    //        });
    //    }
    //    else
    //        Ext.Msg.alert('訊息', '請至少編輯一筆實際採購量');
    //}
    function DeleteRec() {
        var records = T1Grid.getSelection();
        var submitS = ''; // 單筆資料
        var submitT = ''; // 全部資料
        var popMsg = '';
        if (records.length > 0) {
            for (var i = 0; i < records.length; i++) {
                if (records[i].get('ISTRAN') != 'Y' && records[i].get('ISTRAN') != 'T') {
                    submitS = records[i].get('MMCODE');
                    if (i == records.length - 1)
                        submitT += submitS;
                    else
                        submitT += submitS + "ˋ";
                } else {
                    popMsg += records[i].get('MMCODE') + ';';
                }
            }
            if (popMsg == '') {
                Ext.Ajax.request({
                    url: '/api/BD0009/DeleteRec',
                    params: {
                        purdate: T1Query.getForm().findField('P1').rawValue,
                        submitt: submitT
                    },
                    method: reqVal_p,
                    success: function (response) {
                        T1Grid.getSelectionModel().deselectAll();
                        reCalc = 'N';
                        T1Load();
                        msglabel('資料刪除完成');
                    },
                    failure: function (response, options) {
                        Ext.Msg.alert('訊息', '網頁已逾時，請重新登入。');
                    }
                });
            }
            else
                Ext.Msg.alert('訊息', popMsg + '這些院內碼[已轉訂單]，不能刪除。');
        }
        else
            Ext.Msg.alert('訊息', '請先選取項目');
    }
    function T1Tran(tranType) {
        var records = T1Grid.getSelection();
        var submitS = ''; // 單筆資料
        var submitT = ''; // 全部資料
        if (records.length > 0) {
            for (var i = 0; i < records.length; i++) {
                submitS = records[i].get('MMCODE') + "^" + records[i].get('WH_NO');
                if (i == records.length - 1)
                    submitT += submitS;
                else
                    submitT += submitS + "ˋ";
            }
            Ext.Ajax.request({
                url: tranSet,
                params: {
                    purdate: T1Query.getForm().findField('P1').getValue(),
                    submitt: submitT,
                    trantype: tranType
                },
                method: reqVal_p,
                success: function (response) {
                    var data = Ext.decode(response.responseText);
                    if (data.success) {
                        if (data.msg == '000') {
                            T1Grid.getSelectionModel().deselectAll();
                            reCalc = 'N';
                            T1Load();
                            msglabel('轉出訂單完成');
                        }
                        else
                            Ext.Msg.alert('訊息', '藥品採購申請作業轉出訂單執行失敗');
                    } else {
                        Ext.Msg.alert('訊息', data.msg);
                    }

                },
                failure: function (response, options) {
                    Ext.Msg.alert('訊息', '網頁已逾時，請重新登入。');
                }
            });
        }
        else
            Ext.Msg.alert('訊息', '請先選取項目');
    }

    // 勾選TODAYFLAG為Y且ISTRAN為空白的資料
    function selectIsToday() {
        for (var i = 0; i < T1Store.data.items.length; i++) {
            if (T1Store.data.items[i] != null) {
                if (T1Store.data.items[i].data['TODAYFLAG'] == 'Y' && (T1Store.data.items[i].data['ISTRAN'] == 'N' || T1Store.data.items[i].data['ISTRAN'] == ''))
                    checkboxT1Model.select(i, true);
            }
        }
    }

    function updateUpUser() {
        Ext.Ajax.request({
            url: updateUpdateUser,
            params: {
                purdate: T1Query.getForm().findField('P1').rawValue
            },
            method: reqVal_p,
            success: function (response) {
                var data = Ext.decode(response.responseText);
                reCalc = 'N';
                T1Load();
                msglabel('儲存完成');
            },
            failure: function (response, options) {
                Ext.Msg.alert('訊息', '網頁已逾時，請重新登入。');
            }
        });
    }
    function DoClearData() {
        Ext.Ajax.request({
            url: ClearData,
            params: {
                purdate: T1Query.getForm().findField('P1').rawValue
            },
            method: reqVal_p,
            success: function (response) {
                var data = Ext.decode(response.responseText);
                reCalc = 'N';
                reCalcDisRatio = 'N';
                getLowFlag = 'N';
                T1Load();
                msglabel('清空完成');
            },
            failure: function (response, options) {
                msglabel('清空失敗');
                Ext.Msg.alert('訊息', '網頁已逾時，請重新登入。');
            }
        });
    }
    function setTotalCntPrice() {
        //Ext.Ajax.request({
        //    url: totalCntPriceGet,
        //    params: {
        //        purdate: T1Query.getForm().findField('P1').rawValue
        //    },
        //    method: reqVal_p,
        //    success: function (response) {
        //        var data = Ext.decode(response.responseText);
        //        T1Query.getForm().findField('D1').setValue(data.msg.split('_')[0]);
        //        T1Query.getForm().findField('D2').setValue(data.msg.split('_')[1]);
        //    },
        //    failure: function (response, options) {

        //    }
        //});
        T1Query.getForm().findField('D1').setValue(T1Store.data.items.length);
        var priceSum = 0;
        for (var i = 0; i < T1Store.data.items.length; i++) {
            if (parseFloat(T1Store.data.items[i].data.PO_QTY) > 0 && parseFloat(T1Store.data.items[i].data.DISC_CPRICE) > 0)
                priceSum += Math.round(parseFloat(T1Store.data.items[i].data.PO_QTY) * parseFloat(T1Store.data.items[i].data.DISC_CPRICE));
        }
        T1Query.getForm().findField('D2').setValue(parseInt(priceSum));
    }

    function showReport() {
        if (!win) {
            var winform = Ext.create('Ext.form.Panel', {
                id: 'iframeReport',
                //height: '100%',
                //width: '100%',
                layout: 'fit',
                closable: false,
                html: '<iframe src="' + reportUrl + '?WH_NO=' + T1Query.getForm().findField('P0').getValue() + '&PURDATE=' + T1Query.getForm().findField('P1').rawValue + '" id="mainContent" name="mainContent" width="100%" height="100%" frameborder="0" style="background-color:#FFFFFF"></iframe>',
                buttons: [{
                    text: '關閉',
                    margin: '0 20 30 0',
                    handler: function () {
                        this.up('window').destroy();
                    }
                }]
            });
            var win = GetPopWin(viewport, winform, '', viewport.width - 20, viewport.height - 20);
        }
        win.show();
    }

    var callableWin = null;
    popItemImport = function () {
        if (!callableWin) {
            // 廠商清單
            var Agen_noQueryStore = Ext.create('Ext.data.Store', {
                fields: ['VALUE', 'TEXT']
            });
            var T2Store = Ext.create('Ext.data.Store', {
                // autoLoad:true,
                model: 'T2Model',
                pageSize: 20000,
                remoteSort: true,
                sorters: [{ property: 'MMCODE', direction: 'ASC' }],
                listeners: {
                    beforeload: function (store, options) {
                        var np = {
                            p0: T1Query.getForm().findField('P0').getValue(),
                            p1: T1Query.getForm().findField('P1').getValue(),
                            p2: T2Query.getForm().findField('P2').getValue(),
                            pm: T2Query.getForm().findField('PM').getValue(),
                            agen_no: T2Query.getForm().findField('AGEN_NO').getValue()
                        };
                        Ext.apply(store.proxy.extraParams, np);
                    },
                    load: function (store, records, successful, operation, eOpts) {
                        // 載入完成且有資料前disable按鈕
                        if (records.length > 0) {
                            popMainform.down('#seltoday').setDisabled(false);
                            popMainform.down('#selsubmit').setDisabled(false);
                        }
                        else {
                            popMainform.down('#seltoday').setDisabled(true);
                            popMainform.down('#selsubmit').setDisabled(true);
                        }
                    }
                },
                proxy: {
                    type: 'ajax',
                    timeout: 90000,
                    actionMethods: {
                        read: 'POST' // by default GET
                    },
                    url: '/api/BD0009/GetImportItems',
                    reader: {
                        type: 'json',
                        rootProperty: 'etts',
                        totalProperty: 'rc'
                    }
                }

            });
            function setAgenComboData() {
                Ext.Ajax.request({
                    url: '/api/AA0068/GetAgen_NoCombo',
                    method: reqVal_p,
                    success: function (response) {
                        var data = Ext.decode(response.responseText);
                        if (data.success) {
                            var agen_no = data.etts;
                            if (agen_no.length > 0) {
                                for (var i = 0; i < agen_no.length; i++) {
                                    Agen_noQueryStore.add({ VALUE: agen_no[i].VALUE, TEXT: agen_no[i].TEXT });
                                }
                            }
                        }
                    },
                    failure: function (response, options) {
                        Ext.Msg.alert('訊息', '網頁已逾時，請重新登入。');
                    }
                });
            }
            setAgenComboData();
            var T2Query = Ext.widget({
                xtype: 'form',
                layout: 'form',
                border: false,
                autoScroll: true,
                width: '100%',
                collapsible: true,
                hideCollapseTool: true,
                titleCollapse: true,
                fieldDefaults: {
                    xtype: 'textfield',
                    labelAlign: 'right',
                    labelWidth: 70,
                    width: 220
                },
                items: [{
                    xtype: 'container',
                    layout: 'hbox',
                    padding: '2vmin',
                    scrollable: true,
                    items: [
                        {
                            xtype: 'combo',
                            fieldLabel: '院內碼範圍',
                            name: 'PM',
                            id: 'PM',
                            store: MMCODEStore,
                            queryMode: 'local',
                            displayField: 'TEXT',
                            valueField: 'VALUE',
                            multiSelect: true,
                            allowBlank: false, // 欄位為必填
                            fieldCls: 'required',
                            labelAlign: 'right',
                            width: 300
                        }, {
                            xtype: 'combo',
                            store: lTypeStore,
                            displayField: 'TEXT',
                            valueField: 'VALUE',
                            id: 'P2',
                            name: 'P2',
                            margin: '2 2 2 2',
                            fieldLabel: '載入類別',
                            fieldCls: 'required',
                            allowBlank: false,
                            queryMode: 'local',
                            autoSelect: true,
                            value: '1',
                            width: 330,
                            tpl: '<tpl for="."><div class="x-boundlist-item" style="height:auto;">{TEXT}&nbsp;</div></tpl>',
                            editable: true, typeAhead: true, forceSelection: true, selectOnFocus: true,
                            listeners: {
                                focus: function (field, event, eOpts) {
                                    if (!field.isExpanded) {
                                        setTimeout(function () {
                                            field.expand();
                                        }, 300);
                                    }
                                }
                            }
                        }, {
                            xtype: 'combo',
                            fieldLabel: '廠商',
                            name: 'AGEN_NO',
                            id: 'AGEN_NO',
                            enforceMaxLength: true,
                            labelWidth: 60,
                            width: 200,
                            store: Agen_noQueryStore,
                            displayField: 'TEXT',
                            valueField: 'VALUE',
                            queryMode: 'local',
                            anyMatch: true,
                            tpl: '<tpl for="."><div class="x-boundlist-item" style="height:auto;">{TEXT}&nbsp;</div></tpl>'
                        }, {
                            xtype: 'button',
                            text: '查詢',
                            margin: '2 2 2 2',
                            handler: function () {
                                T2Load();
                            }
                        }, {
                            xtype: 'button',
                            itemId: 'seltoday',
                            text: '選取本日出帳',
                            margin: '2 2 2 2',
                            disabled: true,
                            handler: function () {
                                // 先disable按鈕
                                popMainform.down('#seltoday').setDisabled(true);
                                popMainform.down('#selsubmit').setDisabled(true);
                                popMainform.down('#T2Grid').mask('處理中...');

                                setTimeout(function () {
                                    var sm = popMainform.down('#T2Grid').getSelectionModel();
                                    sm.deselectAll();
                                    for (var i = 0; i < T2Store.data.items.length; i++) {
                                        if (T2Store.data.items[i].data.TODAYFLAG == 'V') {
                                            sm.select(i, true, false);
                                        }

                                    }
                                    // 勾選處理完後才enable
                                    popMainform.down('#seltoday').setDisabled(false);
                                    popMainform.down('#selsubmit').setDisabled(false);
                                    popMainform.down('#T2Grid').unmask();
                                }, 300);
                            }
                        }, {
                            xtype: 'button',
                            itemId: 'selsubmit',
                            text: '確定',
                            margin: '2 2 2 2',
                            disabled: true,
                            handler: function () {
                                var selection = popMainform.down('#T2Grid').getSelection();
                                if (selection.length === 0) {
                                    Ext.Msg.alert('提醒', '尚未勾選任何品項');
                                }
                                else {
                                    let mmcode = '';
                                    let adviseqty = '';
                                    let inv_qty = '';
                                    let apl_outqty = '';
                                    let apl_inqty = '';
                                    let safe_qty = '';
                                    let oper_qty = '';
                                    let ship_qty = '';
                                    let high_qty = '';
                                    let min_ordqty = '';
                                    let low_qty = '';
                                    let allqty = '';
                                    let e_purtype = '';
                                    let contracno = '';
                                    let agen_no = '';
                                    let disc_cprice = '';
                                    let po_price = '';
                                    let m_discperc = '';
                                    let unit_swap = '';
                                    let pack_qty0 = '';
                                    let m_purun = '';
                                    $.map(selection, function (item, key) {
                                        mmcode += item.get('MMCODE') + '^';
                                        adviseqty += item.get('ADVISEQTY') + '^';
                                        inv_qty += item.get('INV_QTY') + '^';
                                        apl_outqty += item.get('APL_OUTQTY') + '^';
                                        apl_inqty += item.get('APL_INQTY') + '^';
                                        safe_qty += item.get('SAFE_QTY') + '^';
                                        oper_qty += item.get('OPER_QTY') + '^';
                                        ship_qty += item.get('SHIP_QTY') + '^';
                                        high_qty += item.get('HIGH_QTY') + '^';
                                        min_ordqty += item.get('MIN_ORDQTY') + '^';
                                        low_qty += item.get('LOW_QTY') + '^';
                                        allqty += item.get('ALLQTY') + '^';
                                        e_purtype += item.get('E_PURTYPE') + '^';
                                        contracno += item.get('CONTRACNO') + '^';
                                        agen_no += item.get('AGEN_NO') + '^';
                                        po_price += item.get('PO_PRICE') + '^';
                                        disc_cprice += item.get('DISC_CPRICE') + '^';
                                        m_discperc += item.get('M_DISCPERC') + '^';
                                        unit_swap += item.get('UNIT_SWAP') + '^';
                                        pack_qty0 += item.get('PACK_QTY0') + '^';
                                        m_purun += item.get('M_PURUN') + '^';
                                    });
                                    Ext.MessageBox.confirm('確認', '是否確定載入勾選的品項?', function (btn, text) {
                                        if (btn === 'yes') {
                                            Ext.Ajax.request({
                                                url: '/api/BD0009/ImportSubmit',
                                                method: reqVal_p,
                                                params: {
                                                    WH_NO: T1Query.getForm().findField('P0').getValue(),
                                                    PURDATE: T1Query.getForm().findField('P1').rawValue,
                                                    MMCODE: mmcode,
                                                    ADVISEQTY: adviseqty,
                                                    INV_QTY: inv_qty,
                                                    APL_OUTQTY: apl_outqty,
                                                    APL_INQTY: apl_inqty,
                                                    SAFE_QTY: safe_qty,
                                                    OPER_QTY: oper_qty,
                                                    SHIP_QTY: ship_qty,
                                                    HIGH_QTY: high_qty,
                                                    MIN_ORDQTY: min_ordqty,
                                                    LOW_QTY: low_qty,
                                                    ALLQTY: allqty,
                                                    E_PURTYPE: e_purtype,
                                                    CONTRACNO: contracno,
                                                    AGEN_NO: agen_no,
                                                    PO_PRICE: po_price,
                                                    DISC_CPRICE: disc_cprice,
                                                    M_DISCPERC: m_discperc,
                                                    UNIT_SWAP: unit_swap,
                                                    PACK_QTY0: pack_qty0,
                                                    M_PURUN: m_purun
                                                },
                                                //async: true,
                                                success: function (response) {
                                                    var data = Ext.decode(response.responseText);
                                                    if (data.success) {
                                                        Ext.Msg.alert('訊息', '載入' + selection.length + '品項完成', function () {
                                                            T2Query.down('#T2Close').click();
                                                            T1Load();
                                                        });
                                                    }
                                                    else
                                                        Ext.MessageBox.alert('錯誤', data.msg);
                                                },
                                                failure: function (response) {
                                                    Ext.MessageBox.alert('錯誤', '發生例外錯誤');
                                                }
                                            });
                                        }
                                    }
                                    );
                                }
                            }
                        }, {
                            xtype: 'button',
                            itemId: 'T2Close',
                            text: '關閉',
                            margin: '2 4 2 4',
                            handler: function () {
                                this.up('window').destroy();
                                callableWin = null;
                            }
                        }
                    ]
                }]
            });
            function T2Load() {
                T2Tool.moveFirst();
            }
            var T2Tool = Ext.create('Ext.PagingToolbar', {
                store: T2Store,
                displayInfo: true,
                border: false,
                plain: true
            });
            var checkboxT2Model = Ext.create('Ext.selection.CheckboxModel', {
                checkOnly: true,
                //checkSelector: '.' + Ext.baseCSSPrefix,
                injectCheckbox: 'first',
                mode: 'SIMPLE'
            });

            var popMainform = Ext.create('Ext.panel.Panel', {
                height: '100%',
                width: '90%',
                closable: false,
                plain: true,
                loadMask: true,
                layout: 'fit',
                items: [{
                    xtype: 'grid',
                    itemId: 'T2Grid',
                    store: T2Store,
                    dockedItems: [{
                        dock: 'top',
                        xtype: 'toolbar',
                        items: [T2Query]
                    }],
                    plain: true,
                    loadingText: '處理中...',
                    loadMask: true,
                    //plugins: 'bufferedrenderer',
                    cls: 'T1',
                    selModel: checkboxT2Model,
                    selType: 'checkboxmodel',
                    columns: [
                        {
                            xtype: 'rownumberer',
                            width: 40
                        }, {
                            text: "疫苗",
                            dataIndex: 'E_VACCINE',
                            width: 40,
                            plugins: ''
                        }, {
                            text: "管藥",
                            dataIndex: 'E_RESTRICTCODE',
                            width: 40,
                            plugins: ''
                        }, {
                            text: "院內碼",
                            dataIndex: 'MMCODE',
                            width: 70,
                            plugins: ''
                        }, {
                            text: "藥品名稱",
                            dataIndex: 'MMNAME_E',
                            width: 150,
                            plugins: ''
                        }, {
                            text: "庫存量",
                            dataIndex: 'INV_QTY',
                            align: 'right',
                            style: 'text-align:right',
                            width: 60
                        }, {
                            text: "作業量",
                            dataIndex: 'OPER_QTY',
                            align: 'right',
                            style: 'text-align:right',
                            width: 70
                        }, {
                            text: "安全存量",
                            dataIndex: 'SAFE_QTY',
                            align: 'right',
                            style: 'text-align:right',
                            width: 70
                        }, {
                            text: "最低庫存量",
                            dataIndex: 'LOW_QTY',
                            align: 'right',
                            style: 'text-align:right',
                            width: 80
                        }, {
                            text: "建議採購量",
                            dataIndex: 'ADVISEQTY',
                            align: 'right',
                            style: 'text-align:right',
                            width: 80
                        }, {
                            text: "合約碼",
                            dataIndex: 'CONTRACNO',
                            align: 'right',
                            style: 'text-align:right',
                            width: 70
                        }, {
                            text: "累計進貨量(本月)",
                            align: 'right',
                            style: 'text-align:center',
                            dataIndex: 'APL_INQTY',
                            width: 60
                        }, {
                            text: "累計消耗量(本月)",
                            align: 'right',
                            style: 'text-align:center',
                            dataIndex: 'APL_OUTQTY',
                            width: 60
                        }, {
                            text: "醫令扣庫(本月)",
                            align: 'right',
                            style: 'text-align:center',
                            dataIndex: 'USEQTY',
                            width: 60
                            //}, {
                            //    text: "退藥量(本月)",
                            //    align: 'right',
                            //    style: 'text-align:center',
                            //    dataIndex: 'BACKQTY',
                            //    width: 60
                        }, {
                            text: "全院存量",
                            dataIndex: 'ALLQTY',
                            align: 'center',
                            style: 'text-align:left',
                            width: 40
                        }, {
                            text: "本日出帳",
                            dataIndex: 'TODAYFLAG',
                            align: 'center',
                            style: 'text-align:left',
                            width: 40
                        }, {
                            text: "案別",
                            dataIndex: 'PURTYPE',
                            width: 50
                        }, {
                            text: "廠商",
                            dataIndex: 'AGEN_NAME',
                            width: 100
                        }, {
                            text: " ",
                            align: 'left',
                            flex: 1
                        }, {
                            xtype: 'hiddenfield',
                            dataIndex: 'E_PURTYPE',
                            width: 100
                        }, {
                            xtype: 'hiddenfield',
                            dataIndex: 'CONTRACNO',
                            width: 100
                        }, {
                            xtype: 'hiddenfield',
                            dataIndex: 'AGEN_NO',
                            width: 100
                        }, {
                            xtype: 'hiddenfield',
                            dataIndex: 'PO_PRICE',
                            width: 100
                        }, {
                            xtype: 'hiddenfield',
                            dataIndex: 'DISC_CPRICE',
                            width: 100
                        }, {
                            xtype: 'hiddenfield',
                            dataIndex: 'DISAMOUNT',
                            width: 100
                        }, {
                            xtype: 'hiddenfield',
                            dataIndex: 'UNIT_SWAP',
                            width: 100
                        }, {
                            xtype: 'hiddenfield',
                            dataIndex: 'PACK_QTY0',
                            width: 100
                        }, {
                            xtype: 'hiddenfield',
                            dataIndex: 'M_PURUN',
                            width: 100
                        }
                    ]
                }]
            });

            callableWin = GetPopWin(viewport, popMainform, '載入', viewport.width * 0.9, viewport.height * 0.9);
        }
        T2Query.getForm().findField('PM').setValue(arrPM);
        callableWin.show();
        T2Store.load();
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
        }]
    });

    Ext.getDoc().dom.title = T1Name;
    T1Query.getForm().findField('P1').setValue(new Date());
    getCalcData();
    function ChkMmPoT() {
        var ret = "";
        Ext.Ajax.request({
            url: '/api/BD0009/ChkMmPoT',
            params: {
                purdate: T1Query.getForm().findField('P1').rawValue
            },
            method: reqVal_p,
            success: function (response) {
                var data = Ext.decode(response.responseText);
                ret = data.msg;
            },
            failure: function (response, options) {
                Ext.Msg.alert('訊息', '網頁已逾時，請重新登入。');
            }
        });
        return ret;
    }
});

 Ext.on('mouseclick', function () {
        windowHeight = $(window).height();
        windowWidth = $(window).width();
        popformINI.setHeight(windowHeight);
        popformINI.setWidth(windowWidth);
        multiCheckWindow.setHeight(windowHeight);
        multiCheckWindow.setWidth(windowWidth);
    });